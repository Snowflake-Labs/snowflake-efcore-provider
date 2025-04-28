using System;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Snowflake.EntityFrameworkCore;
using Snowflake.EntityFrameworkCore.Diagnostics.Internal;
using Snowflake.EntityFrameworkCore.Extensions;
using Snowflake.EntityFrameworkCore.Infrastructure;
using Snowflake.EntityFrameworkCore.Infrastructure.Internal;
using Snowflake.EntityFrameworkCore.Metadata.Conventions;
using Snowflake.EntityFrameworkCore.Metadata.Internal;
using Snowflake.EntityFrameworkCore.Migrations;
using Snowflake.EntityFrameworkCore.Migrations.Internal;
using Snowflake.EntityFrameworkCore.Query.Internal;
using Snowflake.EntityFrameworkCore.Storage.Internal;
using Snowflake.EntityFrameworkCore.Update.Internal;
using Snowflake.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Snowflake specific extension methods for <see cref="IServiceCollection" />.
/// </summary>
public static class SnowflakeServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the given Entity Framework <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />
    ///     and configures it to connect to a Snowflake database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is a shortcut for configuring a <see cref="DbContext" /> to use Snowflake. It does not support all options.
    ///         Use <see cref="O:EntityFrameworkServiceCollectionExtensions.AddDbContext" /> and related methods for full control of
    ///         this process.
    ///     </para>
    ///     <para>
    ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
    ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
    ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
    ///         overridden to configure the Snowflake provider and connection string.
    ///     </para>
    ///     <para>
    ///         To configure the <see cref="DbContextOptions{TContext}" /> for the context, either override the
    ///         <see cref="DbContext.OnConfiguring" /> method in your derived context, or supply
    ///         an optional action to configure the <see cref="DbContextOptions" /> for the context.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///         <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be registered.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="SnowflakeOptionsAction">An optional action to allow additional Snowflake specific configuration.</param>
    /// <param name="optionsAction">An optional action to configure the <see cref="DbContextOptions" /> for the context.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSnowflake<TContext>(
        this IServiceCollection serviceCollection,
        string? connectionString,
        Action<SnowflakeDbContextOptionsBuilder>? SnowflakeOptionsAction = null,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
        => serviceCollection.AddDbContext<TContext>(
            (_, options) =>
            {
                optionsAction?.Invoke(options);
                options.UseSnowflake(connectionString, SnowflakeOptionsAction);
            });

    /// <summary>
    ///     <para>
    ///         Adds the services required by the Microsoft Snowflake database provider for Entity Framework
    ///         to an <see cref="IServiceCollection" />.
    ///     </para>
    ///     <para>
    ///         Warning: Do not call this method accidentally. It is much more likely you need
    ///         to call <see cref="AddSnowflake{TContext}" />.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     Calling this method is no longer necessary when building most applications, including those that
    ///     use dependency injection in ASP.NET or elsewhere.
    ///     It is only needed when building the internal service provider for use with
    ///     the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
    ///     This is not recommend other than for some advanced scenarios.
    /// </remarks>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>
    ///     The same service collection so that multiple calls can be chained.
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IServiceCollection AddEntityFrameworkSnowflake(this IServiceCollection serviceCollection)
    {
        new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<IMigrationsModelDiffer, SnowflakeMigrationsModelDiffer>()
            .TryAdd<LoggingDefinitions, SnowflakeLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<SnowflakeOptionsExtension>>()
            .TryAdd<IValueGeneratorCache>(p => p.GetRequiredService<ISnowflakeValueGeneratorCache>())
            .TryAdd<IRelationalTypeMappingSource, SnowflakeTypeMappingSource>()
            .TryAdd<ISqlGenerationHelper, SnowflakeSqlGenerationHelper>()
            .TryAdd<IRelationalAnnotationProvider, SnowflakeAnnotationProvider>()
            .TryAdd<IMigrationsAnnotationProvider, SnowflakeMigrationsAnnotationProvider>()
            .TryAdd<IModelValidator, SnowflakeModelValidator>()
            .TryAdd<IProviderConventionSetBuilder, SnowflakeConventionSetBuilder>()
            .TryAdd<IUpdateSqlGenerator>(p => p.GetRequiredService<ISnowflakeUpdateSqlGenerator>())
            .TryAdd<IEvaluatableExpressionFilter, SnowflakeEvaluatableExpressionFilter>()
            .TryAdd<IRelationalTransactionFactory, SnowflakeTransactionFactory>()
            .TryAdd<IModificationCommandBatchFactory, SnowflakeModificationCommandBatchFactory>()
            .TryAdd<IModificationCommandFactory, SnowflakeModificationCommandFactory>()
            .TryAdd<IValueGeneratorSelector, SnowflakeValueGeneratorSelector>()
            .TryAdd<IRelationalConnection>(p => p.GetRequiredService<ISnowflakeConnection>())
            .TryAdd<IMigrationsSqlGenerator, SnowflakeMigrationsSqlGenerator>()
            .TryAdd<IRelationalDatabaseCreator, SnowflakeDatabaseCreator>()
            .TryAdd<IHistoryRepository, SnowflakeHistoryRepository>()
            .TryAdd<IExecutionStrategyFactory, SnowflakeExecutionStrategyFactory>()
            .TryAdd<IRelationalQueryStringFactory, SnowflakeQueryStringFactory>()
            .TryAdd<ICompiledQueryCacheKeyGenerator, SnowflakeCompiledQueryCacheKeyGenerator>()
            .TryAdd<IQueryCompilationContextFactory, SnowflakeQueryCompilationContextFactory>()
            .TryAdd<IMethodCallTranslatorProvider, SnowflakeMethodCallTranslatorProvider>()
            .TryAdd<IAggregateMethodCallTranslatorProvider, SnowflakeAggregateMethodCallTranslatorProvider>()
            .TryAdd<IMemberTranslatorProvider, SnowflakeMemberTranslatorProvider>()
            .TryAdd<IQuerySqlGeneratorFactory, SnowflakeQuerySqlGeneratorFactory>()
            .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, SnowflakeSqlTranslatingExpressionVisitorFactory>()
            .TryAdd<ISqlExpressionFactory, SnowflakeSqlExpressionFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, SnowflakeQueryTranslationPostprocessorFactory>()
            .TryAdd<IRelationalParameterBasedSqlProcessorFactory, SnowflakeParameterBasedSqlProcessorFactory>()
            .TryAdd<INavigationExpansionExtensibilityHelper, SnowflakeNavigationExpansionExtensibilityHelper>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, SnowflakeQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<IExceptionDetector, SnowflakeExceptionDetector>()
            .TryAdd<ISingletonOptions, ISnowflakeSingletonOptions>(p => p.GetRequiredService<ISnowflakeSingletonOptions>())
            .TryAdd<IMigrationCommandExecutor, SnowflakeMigrationCommandExecutor>()
            .TryAdd<IDbContextServices, SnowflakeDbContextServices>()
            .TryAddProviderSpecificServices(
                b => b
                    .TryAddSingleton<ISnowflakeSingletonOptions, SnowflakeSingletonOptions>()
                    .TryAddSingleton<ISnowflakeValueGeneratorCache, SnowflakeValueGeneratorCache>()
                    .TryAddSingleton<ISnowflakeUpdateSqlGenerator, SnowflakeUpdateSqlGenerator>()
                    .TryAddSingleton<ISnowflakeSequenceValueGeneratorFactory, SnowflakeSequenceValueGeneratorFactory>()
                    .TryAddSingleton<ISnowflakeAlterColumnGeneratorHelper, SnowflakeAlterColumnGeneratorHelper>()
                    .TryAddSingleton<ISnowflakeHybridTableValidator, SnowflakeHybridTableValidator>()
                    .TryAddSingleton<ISnowflakeIdentityValidator, SnowflakeIdentityValidator>()
                    .TryAddScoped<ISnowflakeConnection, SnowflakeConnection>())
            .TryAddCoreServices();

        return serviceCollection;
    }
}