using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using EFCore.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.Data.Client;
using Snowflake.Data.Core;
using Snowflake.EntityFrameworkCore.Diagnostics;

#pragma warning disable IDE0022 // Use block body for methods
// ReSharper disable SuggestBaseTypeForParameter
namespace Snowflake.EntityFrameworkCore.TestUtilities;

public class SnowflakeTestStore : RelationalTestStore
{
    public const int CommandTimeout = 300;

    private static string CurrentDirectory
        => Environment.CurrentDirectory;
    
    public string Warehouse { get; private set; }

    public static SnowflakeTestStore GetOrCreate(string name)
        => new(name);

    public static SnowflakeTestStore GetOrCreateInitialized(string name)
        => new SnowflakeTestStore(name).InitializeSnowflake(null, (Func<DbContext>)null, null);

    public static SnowflakeTestStore GetOrCreateWithInitScript(string name, string initScript)
        => new(name, initScript: initScript);

    public static SnowflakeTestStore GetOrCreateWithScriptPath(
        string name,
        string scriptPath,
        bool? multipleActiveResultSets = null,
        bool shared = true)
        => new(name, scriptPath: scriptPath, multipleActiveResultSets: multipleActiveResultSets, shared: shared);

    public static SnowflakeTestStore Create(string name, bool useFileName = false)
        => new(name, useFileName, shared: false);

    public static SnowflakeTestStore CreateInitialized(string name, bool? multipleActiveResultSets = null)
        => new SnowflakeTestStore(name, shared: false, multipleActiveResultSets: multipleActiveResultSets)
            .InitializeSnowflake(null, (Func<DbContext>)null, null);

    private readonly string _initScript;
    private readonly string _scriptPath;

    private SnowflakeTestStore(
        string name,
        bool? multipleActiveResultSets = null,
        string initScript = null,
        string scriptPath = null,
        bool shared = true)
        : base(name, shared)
    {

        if (initScript != null)
        {
            _initScript = initScript;
        }

        if (scriptPath != null)
        {
            _scriptPath = Path.Combine(Path.GetDirectoryName(typeof(SnowflakeTestStore).Assembly.Location), scriptPath);
        }

        ConnectionString = TestUtil.GetConnectionStringFromParameters("parameters.json", (t) =>
        {
            t.database = Name;
            t.schema = "PUBLIC";
            this.Warehouse = t.warehouse;
        });
        Connection = new SnowflakeDbConnection(ConnectionString);
    }

    public SnowflakeTestStore InitializeSnowflake(
        IServiceProvider serviceProvider,
        Func<DbContext> createContext,
        Action<DbContext> seed)
        => (SnowflakeTestStore)Initialize(serviceProvider, createContext, seed);

    public SnowflakeTestStore InitializeSnowflake(
        IServiceProvider serviceProvider,
        Func<SnowflakeTestStore, DbContext> createContext,
        Action<DbContext> seed)
        => InitializeSnowflake(serviceProvider, () => createContext(this), seed);

    protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean)
    {
        if (CreateDatabase(clean))
        {
            // Resets connection to use database created just now
            Connection = new SnowflakeDbConnection(CreateConnectionString(Name, (t) => t.database = Name));
            if (_scriptPath != null)
            {
                ExecuteScript(File.ReadAllText(_scriptPath));
            }
            else
            {
                using var context = createContext();
                context.Database.EnsureCreatedResiliently();

                if (_initScript != null)
                {
                    ExecuteScript(_initScript);
                }

                seed?.Invoke(context);
            }
        }
    }

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        => builder
            .UseSnowflake(
                Connection, b => 
                    b.ApplyConfiguration()
                        .CommandTimeout(CommandTimeout)
                        .ReverseNullOrdering()
                    )
            .ConfigureWarnings(b =>
            {
                b.Ignore(SnowflakeEventId.SavepointsDisabledBecauseOfMARS);
                b.Ignore(SnowflakeEventId.StandardTableWarning);
                b.Ignore(SnowflakeEventId.DecimalTypeKeyWarning);
                b.Ignore(SnowflakeEventId.DecimalTypeDefaultWarning);
            }); 

    private bool CreateDatabase(Action<DbContext> clean)
    {
        using var master = new SnowflakeDbConnection(CreateConnectionString("master"));
        if (ExecuteScalar<long>(master, $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.DATABASES WHERE DATABASE_NAME = '{Name.ToUpper()}'") > 0)
        {
            if (_scriptPath != null)
            {
                return false;
            }

            using var context = new DbContext(
                AddProviderOptions(
                        new DbContextOptionsBuilder()
                            .EnableServiceProviderCaching(false))
                    .Options);
            Clean(context);
            clean?.Invoke(context);
            return true;
        }

        ExecuteNonQuery(master,GetCreateDatabaseStatement(Name));
        WaitForExists((SnowflakeDbConnection)Connection);

        return true;
    }

    public override void Clean(DbContext context)
        => context.Database.EnsureClean();

    public void ExecuteScript(string script) =>
        Execute(Connection, command =>
        {
            var statements =
                new Regex("(?<=^\\s*)------Statement------(?=\\s*$)", RegexOptions.IgnoreCase | RegexOptions.Multiline,
                        TimeSpan.FromMilliseconds(1000.0)).Split(script)
                    .Where(b => !string.IsNullOrEmpty(b));
            foreach (var batch in statements)
            {
                command.CommandText = batch;
                command.ExecuteNonQuery();
            }

            return 0;
        }, "");

    private static void WaitForExists(SnowflakeDbConnection connection)
        => new TestSnowflakeRetryingExecutionStrategy().Execute(connection, WaitForExistsImplementation);

    private static void WaitForExistsImplementation(SnowflakeDbConnection connection)
    {
        var retryCount = 0;
        while (true)
        {
            try
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }

                // TODO REVIEW POOLING
                //SnowflakeDbConnection.ClearPool(connection);

                connection.Open();
                connection.Close();
                return;
            }
            catch (SnowflakeDbException e)
            {
                if (++retryCount >= 30)
                    // TODO CHECK || e.Number != 233 && e.Number != -2 && e.Number != 4060 && e.Number != 1832 && e.Number != 5120)
                {
                    throw;
                }

                Thread.Sleep(100);
            }
        }
    }

    private static string GetCreateDatabaseStatement(string name)
    {
        var result = $"CREATE DATABASE {name}";
        return result;
    }

    public void DeleteDatabase()
    {
        using var master = new SnowflakeDbConnection(CreateConnectionString("master"));
        ExecuteNonQuery(
            master, string.Format(
                @"BEGIN
    IF ( EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.DATABASES WHERE DATABASE_NAME = '{0}')) THEN
      DROP DATABASE {0};
    END IF;
END;", Name));

        // TODO REVIEW
        // SnowflakeDbConnection.ClearAllPools();
    }

    public override void OpenConnection()
        => new TestSnowflakeRetryingExecutionStrategy().Execute(Connection, connection => connection.Open());

    public override Task OpenConnectionAsync()
        => new TestSnowflakeRetryingExecutionStrategy().ExecuteAsync(Connection, connection => connection.OpenAsync());

    public T ExecuteScalar<T>(string sql, params (object,SFDataType)[] parameters)
        => ExecuteScalar<T>(Connection, sql, parameters);

    private static T ExecuteScalar<T>(DbConnection connection, string sql, params (object,SFDataType)[] parameters)
        => Execute(connection, command => (T)command.ExecuteScalar(), sql, false, 1, parameters);

    public Task<T> ExecuteScalarAsync<T>(string sql, params (object,SFDataType)[] parameters)
        => ExecuteScalarAsync<T>(Connection, sql, parameters);

    private static Task<T> ExecuteScalarAsync<T>(DbConnection connection, string sql, IReadOnlyList<(object,SFDataType)> parameters = null)
        => ExecuteAsync(connection, async command => (T)await command.ExecuteScalarAsync(), sql, false, parameters: parameters);

    public int ExecuteNonQuery(string sql, int multiStatementCount = 1, params (object,SFDataType)[] parameters)
        => ExecuteNonQuery(Connection, sql, multiStatementCount, parameters);

    private static int ExecuteNonQuery(DbConnection connection, string sql, int multiStatementCount = 1, (object,SFDataType)[] parameters = null)
        => Execute(connection, command => command.ExecuteNonQuery(), sql, false, multiStatementCount, parameters);

    public Task<int> ExecuteNonQueryAsync(string sql, int multiStatementCount = 1, params (object,SFDataType)[] parameters)
        => ExecuteNonQueryAsync(Connection, sql, multiStatementCount, parameters);

    private static Task<int> ExecuteNonQueryAsync(DbConnection connection, string sql, int multiStatementCount = 1, IReadOnlyList<(object,SFDataType)> parameters = null)
        => ExecuteAsync(connection, command => command.ExecuteNonQueryAsync(), sql, false, multiStatementCount, parameters);

    public IEnumerable<T> Query<T>(string sql, params (object,SFDataType)[] parameters)
        => Query<T>(Connection, sql, parameters:parameters);

    private static IEnumerable<T> Query<T>(DbConnection connection, string sql, int multiStatementCount = 1, (object,SFDataType)[] parameters = null)
        => Execute(
            connection, command =>
            {
                using var dataReader = command.ExecuteReader();
                var results = Enumerable.Empty<T>();
                while (dataReader.Read())
                {
                    results = results.Concat(new[] { dataReader.GetFieldValue<T>(0) });
                }

                return results;
            }, sql, false, multiStatementCount, parameters);

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, int multiStatementCount = 1, params (object,SFDataType)[] parameters)
        => QueryAsync<T>(Connection, sql, multiStatementCount, parameters);

    private static Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql, int multiStatementCount = 1, (object,SFDataType)[] parameters = null)
        => ExecuteAsync(
            connection, async command =>
            {
                using var dataReader = await command.ExecuteReaderAsync();
                var results = Enumerable.Empty<T>();
                while (await dataReader.ReadAsync())
                {
                    results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
                }

                return results;
            }, sql, false, multiStatementCount, parameters);

    private static T Execute<T>(
        DbConnection connection,
        Func<DbCommand, T> execute,
        string sql,
        bool useTransaction = false,
        int multiStatementCount = 1,
        (object, SFDataType)[] parameters = null)
        => new TestSnowflakeRetryingExecutionStrategy().Execute(
            new
            {
                connection,
                execute,
                sql,
                useTransaction,
                multiStatementCount = multiStatementCount,
                parameters
            },
            state => ExecuteCommand(state.connection, state.execute, state.sql, state.useTransaction, state.multiStatementCount, state.parameters));

    private static T ExecuteCommand<T>(
        DbConnection connection,
        Func<DbCommand, T> execute,
        string sql,
        bool useTransaction,
        int multiStatementCount,
        (object, SFDataType)[] parameters)
    {
        if (connection.State != ConnectionState.Closed)
        {
            connection.Close();
        }

        connection.Open();
        try
        {
            using var transaction = useTransaction ? connection.BeginTransaction() : null;
            T result;
            using (var command = CreateCommand(connection, sql, multiStatementCount, parameters))
            {
                command.Transaction = transaction;
                result = execute(command);
            }

            transaction?.Commit();

            return result;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }
    }

    private static Task<T> ExecuteAsync<T>(
        DbConnection connection,
        Func<DbCommand, Task<T>> executeAsync,
        string sql,
        bool useTransaction = false,
        int multiStatementCount = 1,
        IReadOnlyList<(object,SFDataType)> parameters = null)
        => new TestSnowflakeRetryingExecutionStrategy().ExecuteAsync(
            new
            {
                connection,
                executeAsync,
                sql,
                useTransaction,
                multiStatementCount,
                parameters
            },
            state => ExecuteCommandAsync(state.connection, state.executeAsync, state.sql, state.useTransaction, state.multiStatementCount, state.parameters));

    private static async Task<T> ExecuteCommandAsync<T>(
        DbConnection connection,
        Func<DbCommand, Task<T>> executeAsync,
        string sql,
        bool useTransaction,
        int multiStatementCount,
        IReadOnlyList<(object, SFDataType)> parameters)
    {
        if (connection.State != ConnectionState.Closed)
        {
            await connection.CloseAsync();
        }

        await connection.OpenAsync();
        try
        {
            using var transaction = useTransaction ? await connection.BeginTransactionAsync() : null;
            T result;
            using (var command = CreateCommand(connection, sql, multiStatementCount, parameters))
            {
                result = await executeAsync(command);
            }

            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return result;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static DbCommand CreateCommand(
        DbConnection connection,
        string commandText,
        int multiStatementParameter,
        IReadOnlyList<(object,SFDataType)> parameters = null)
    {
        var command = (SnowflakeDbCommand)connection.CreateCommand();

        command.CommandText = commandText;
        command.CommandTimeout = CommandTimeout;

        if (parameters != null)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameterInfo = parameters[i];
                command.Parameters.Add(new SnowflakeDbParameter($"p{i}", parameterInfo.Item2)
                {
                    Value = parameterInfo.Item1
                });
            }
        }

        if (multiStatementParameter == 1) return command;
        
        var stmtCountParam = command.CreateParameter();
        stmtCountParam.ParameterName = "MULTI_STATEMENT_COUNT";
        stmtCountParam.DbType = DbType.Int16;
        stmtCountParam.Value = multiStatementParameter;
        command.Parameters.Add(stmtCountParam);

        return command;
    }

    public override void Dispose()
    {
        base.Dispose();

        DeleteDatabase();
    }

    public static string CreateConnectionString(string name, Action<TestConfig> action = null)
    {
        Action<TestConfig> actionByDefault = (t) =>
        {
            t.schema = string.Empty;
        };
        var connectionString = TestUtil.GetConnectionStringFromParameters("parameters.json", action ?? actionByDefault);
        return connectionString;
    }

    public override FormattableString NormalizeDelimitersInInterpolatedString(FormattableString sql)
    {
        var sqlString = sql.Format;
        var sqlArgs = sql.GetArguments();

        if (sqlArgs.Any(arg => arg is not SnowflakeDbParameter))
        {
            return new TestFormattableString(NormalizeDelimitersInRawString(sqlString), sqlArgs);
        }

        var placeholders = sqlArgs.Select((arg, index) =>
            new { Placeholder = $"{{{index}}}", ParamName = $":{(arg as SnowflakeDbParameter)!.ParameterName}" });

        sqlString = placeholders.Aggregate(sqlString,
            (current, placeholder) => current.Replace(placeholder.Placeholder, placeholder.ParamName));

        return new TestFormattableString(NormalizeDelimitersInRawString(sqlString), sqlArgs);
    }

    public override string NormalizeDelimitersInRawString(string sql)
    {
        sql = sql.Replace("[", OpenDelimiter).Replace("]", CloseDelimiter).Replace("@", ":").Replace("{", ":p")
            .Replace("}", "");
        return sql;
    }
}