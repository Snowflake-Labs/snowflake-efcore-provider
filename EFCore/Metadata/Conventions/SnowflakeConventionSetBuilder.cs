using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Snowflake.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A builder for building conventions for Snowflake.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" /> and multiple registrations
///         are allowed. This means that each <see cref="DbContext" /> instance will use its own
///         set of instances of this service.
///         The implementations may depend on other services registered with any lifetime.
///         The implementations do not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///         <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class SnowflakeConventionSetBuilder : RelationalConventionSetBuilder
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    /// <summary>
    ///     Creates a new <see cref="SnowflakeConventionSetBuilder" /> instance.
    /// </summary>
    /// <param name="dependencies">The core dependencies for this service.</param>
    /// <param name="relationalDependencies">The relational dependencies for this service.</param>
    /// <param name="sqlGenerationHelper">The SQL generation helper to use.</param>
    public SnowflakeConventionSetBuilder(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(dependencies, relationalDependencies)
    {
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    /// <summary>
    ///     Builds and returns the convention set for the current database provider.
    /// </summary>
    /// <returns>The convention set for the current database provider.</returns>
    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        // Snowflake-specific conventions
        conventionSet.Add(new SnowflakeValueGenerationStrategyConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new RelationalMaxIdentifierLengthConvention(128, Dependencies, RelationalDependencies));
        conventionSet.Add(new SnowflakeIndexConvention(Dependencies, RelationalDependencies, _sqlGenerationHelper));
        conventionSet.Add(new SnowflakeMemoryOptimizedTablesConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new SnowflakeDbFunctionConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new SnowflakeHybridTableAttributeConvention(Dependencies));
        
        // Replace existing conventions
        conventionSet.Replace<CascadeDeleteConvention>(
            new SnowflakeOnDeleteConvention(Dependencies, RelationalDependencies));
        conventionSet.Replace<StoreGenerationConvention>(
            new SnowflakeStoreGenerationConvention(Dependencies, RelationalDependencies));
        conventionSet.Replace<ValueGenerationConvention>(
            new SnowflakeValueGenerationConvention(Dependencies, RelationalDependencies));
        conventionSet.Replace<RuntimeModelConvention>(new SnowflakeRuntimeModelConvention(Dependencies, RelationalDependencies));
        conventionSet.Replace<SharedTableConvention>(
            new SnowflakeSharedTableConvention(Dependencies, RelationalDependencies));

        var snowflakeTemporalConvention = new SnowflakeTemporalConvention(Dependencies, RelationalDependencies);
        ConventionSet.AddBefore(
            conventionSet.EntityTypeAnnotationChangedConventions,
            snowflakeTemporalConvention,
            typeof(SnowflakeValueGenerationConvention));
        conventionSet.SkipNavigationForeignKeyChangedConventions.Add(snowflakeTemporalConvention);
        conventionSet.ModelFinalizingConventions.Add(snowflakeTemporalConvention);
        conventionSet.ModelFinalizingConventions.Add(new SnowflakeHybridTableAttributeConvention(Dependencies));
        conventionSet.Replace<ForeignKeyIndexConvention>(new SnowflakeForeignKeyIndexConvention(Dependencies));

        return conventionSet;
    }

    /// <summary>
    ///     Call this method to build a <see cref="ConventionSet" /> for Snowflake when using
    ///     the <see cref="ModelBuilder" /> outside of <see cref="DbContext.OnModelCreating" />.
    /// </summary>
    /// <remarks>
    ///     Note that it is unusual to use this method. Consider using <see cref="DbContext" /> in the normal way instead.
    /// </remarks>
    /// <returns>The convention set.</returns>
    public static ConventionSet Build()
    {
        using var serviceScope = CreateServiceScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
        return ConventionSet.CreateConventionSet(context);
    }

    /// <summary>
    ///     Call this method to build a <see cref="ModelBuilder" /> for Snowflake outside of <see cref="DbContext.OnModelCreating" />.
    /// </summary>
    /// <remarks>
    ///     Note that it is unusual to use this method. Consider using <see cref="DbContext" /> in the normal way instead.
    /// </remarks>
    /// <returns>The convention set.</returns>
    public static ModelBuilder CreateModelBuilder()
    {
        using var serviceScope = CreateServiceScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
        return new ModelBuilder(ConventionSet.CreateConventionSet(context), context.GetService<ModelDependencies>());
    }

    /// <summary>
    ///    Call this method to build a <see cref="ModelBuilder" /> for Snowflake outside of <see cref="DbContext.OnModelCreating" />.
    /// </summary>
    /// <returns></returns>
    private static IServiceScope CreateServiceScope()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSnowflake()
            .AddDbContext<DbContext>(
                (p, o) =>
                    o.UseSnowflake("Server=.")
                        .UseInternalServiceProvider(p))
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
    }
}
