using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Snowflake.EntityFrameworkCore.Infrastructure.Internal;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <inheritdoc />
public class SnowflakeQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISnowflakeSingletonOptions _SnowflakeSingletonOptions;


    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeQuerySqlGeneratorFactory" />
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="typeMappingSource"></param>
    /// <param name="SnowflakeSingletonOptions"></param>
    public SnowflakeQuerySqlGeneratorFactory(
        QuerySqlGeneratorDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource,
        ISnowflakeSingletonOptions SnowflakeSingletonOptions)
    {
        Dependencies = dependencies;
        _typeMappingSource = typeMappingSource;
        _SnowflakeSingletonOptions = SnowflakeSingletonOptions;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual QuerySqlGeneratorDependencies Dependencies { get; }


    /// <inheritdoc />
    public virtual QuerySqlGenerator Create()
        => new SnowflakeQuerySqlGenerator(Dependencies, _typeMappingSource, _SnowflakeSingletonOptions);
}