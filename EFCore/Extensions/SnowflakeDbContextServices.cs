using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Snowflake.EntityFrameworkCore.Infrastructure.Internal;
using Snowflake.EntityFrameworkCore.Internal;

namespace Snowflake.EntityFrameworkCore.Extensions;

public class SnowflakeDbContextServices : DbContextServices
{
    public override IDbContextServices Initialize(
        IServiceProvider scopedProvider,
        DbContextOptions contextOptions,
        DbContext context)
    {
#pragma warning disable EF1001        
        var dbContextServices = base.Initialize(scopedProvider, contextOptions, context);
#pragma warning restore EF1001       

        if (IsSensitiveLoggingEnabledOnNonDevelopmentEnvironment(contextOptions.Extensions, out var environmentType))
        {
            var diagnostics = scopedProvider.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>();
            var definition = SnowflakeResources.LogSensitiveDataLoggingOnNonDevelopmentEnvironment(diagnostics);
            definition.Log(diagnostics, environmentType.ToString());
        }
                
        return dbContextServices;
    }

    private bool IsSensitiveLoggingEnabledOnNonDevelopmentEnvironment(IEnumerable<IDbContextOptionsExtension> extensions, out EnvironmentType environmentType)
    {
        EnvironmentType environment = EnvironmentType.Unknown;
        bool sensitiveLogging = false;
        foreach (var extension in extensions)
        {
            
            switch (extension)
            {
                case SnowflakeOptionsExtension sfExt:
                    environment = sfExt.EnvironmentType;
                    break;
                case CoreOptionsExtension coreExt:
                    sensitiveLogging = coreExt.IsSensitiveDataLoggingEnabled;
                    break;
            }
        }

        environmentType = environment;
        return environment != EnvironmentType.Development && sensitiveLogging;
    }
}