namespace Snowflake.EntityFrameworkCore.Query.Internal;

using Microsoft.EntityFrameworkCore.Query;

public class SnowflakeAggregateMethodCallTranslatorProvider : RelationalAggregateMethodCallTranslatorProvider
{
    
    public SnowflakeAggregateMethodCallTranslatorProvider(RelationalAggregateMethodCallTranslatorProviderDependencies dependencies)
        : base(dependencies)
    {
        var sqlExpressionFactory = dependencies.SqlExpressionFactory;
        var typeMappingSource = dependencies.RelationalTypeMappingSource;

        AddTranslators(
            new IAggregateMethodCallTranslator[]
            {
                new SnowflakeLongCountMethodTranslator(sqlExpressionFactory),
                new SnowflakeStatisticsAggregateMethodTranslator(sqlExpressionFactory, typeMappingSource),
                new SnowflakeStringAggregateMethodTranslator(sqlExpressionFactory, typeMappingSource)
            });
    }
}
