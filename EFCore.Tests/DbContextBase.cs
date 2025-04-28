using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Snowflake.EntityFrameworkCore;
using Snowflake.EntityFrameworkCore.Extensions;
using Snowflake.EntityFrameworkCore.Infrastructure;
using Snowflake.EntityFrameworkCore.Infrastructure.Internal;

namespace EFCore.Tests;

public class HasTablesOperationInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        // to force EnsureCreate used within tests create a test table even another table got already created (in different test case)
        // suboptimal cause we create table each test even when same table already exists
        // TODO: migrations to embrace instead of EnsureCreated
        if (command.CommandText.Contains("DECODE(COUNT(*), 0, 0, 1) FROM INFORMATION_SCHEMA.TABLES"))
            return InterceptionResult<object>.SuppressWithResult(0L);

        return result;
    }
}

public class DbContextBase : DbContext
{
    private string? _connectionString;
    private ILoggerFactory? _loggerFactory;

    public DbContextBase()
    {
        _connectionString = GetConnectionStringFromParameters("parameters.json");
        Log4NetFactory.LoggerInit("app.config");
        _loggerFactory = Log4NetFactory.LoggerFactory();
    }

    public string? ConnectionString => _connectionString;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSnowflake(_connectionString);
        options.UseEnvironment(EnvironmentType.Development);
        options.UseLoggerFactory(_loggerFactory);
        options.AddInterceptors(new HasTablesOperationInterceptor());
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
    
    public static string GetConnectionStringFromParameters(string parametersJson)
    {
        string connectionString;
        var reader = new StreamReader(parametersJson);
        var testConfigString = reader.ReadToEnd();
        // Local JSON settings to avoid using system wide settings which could be different 
        // than the default ones
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new DefaultNamingStrategy()
            }
        };
        var testConfigs = JsonConvert.DeserializeObject<Dictionary<string, TestConfig>>(testConfigString, jsonSettings);
        if (testConfigs == null)
        {
            throw new NoNullAllowedException(nameof(testConfigs));
        }
        if (testConfigs.TryGetValue("testconnection", out var testConnectionConfig))
        {
            connectionString = testConnectionConfig.ConnectionString;
        }
        else
        {
            throw new NoNullAllowedException($"null `testconnection` in {parametersJson} config file");
        }

        return connectionString;
    }
}
