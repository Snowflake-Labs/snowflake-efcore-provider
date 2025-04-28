using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <inheritdoc />
public class SnowflakeQueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeQueryTranslationPostprocessorFactory" />
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    /// <param name="typeMappingSource"></param>
    public SnowflakeQueryTranslationPostprocessorFactory(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        IRelationalTypeMappingSource typeMappingSource)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
        _typeMappingSource = typeMappingSource;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual QueryTranslationPostprocessorDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryTranslationPostprocessorDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
        => new SnowflakeQueryTranslationPostprocessor(Dependencies, RelationalDependencies, queryCompilationContext, _typeMappingSource);
}
