using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Snowflake specific member translator provider
/// </summary>
public class SnowflakeMemberTranslatorProvider : RelationalMemberTranslatorProvider
{
    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeMemberTranslatorProvider" />
    /// </summary>
    public SnowflakeMemberTranslatorProvider(
        RelationalMemberTranslatorProviderDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource)
        : base(dependencies)
    {
        var sqlExpressionFactory = dependencies.SqlExpressionFactory;

        AddTranslators(
        [
            new SnowflakeDateOnlyMemberTranslator(sqlExpressionFactory),
            new SnowflakeDateTimeMemberTranslator(sqlExpressionFactory, typeMappingSource),
            new SnowflakeStringMemberTranslator(sqlExpressionFactory),
            new SnowflakeTimeSpanMemberTranslator(sqlExpressionFactory),
            new SnowflakeTimeOnlyMemberTranslator(sqlExpressionFactory)
        ]);
    }
}