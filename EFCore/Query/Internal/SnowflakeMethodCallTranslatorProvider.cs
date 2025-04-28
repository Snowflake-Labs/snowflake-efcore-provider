using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// snowflake method call translator provider
/// </summary>
public class SnowflakeMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
{
    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeMethodCallTranslatorProvider" />
    /// </summary>
    /// <param name="dependencies"></param>
    public SnowflakeMethodCallTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies)
        : base(dependencies)
    {
        var sqlExpressionFactory = dependencies.SqlExpressionFactory;
        var typeMappingSource = dependencies.RelationalTypeMappingSource;
        AddTranslators(
        [
            new SnowflakeByteArrayMethodTranslator(sqlExpressionFactory),
            new SnowflakeConvertTranslator(sqlExpressionFactory, typeMappingSource),
            new SnowflakeDataLengthFunctionTranslator(sqlExpressionFactory),
            new SnowflakeDateDiffFunctionsTranslator(sqlExpressionFactory),
            new SnowflakeDateOnlyMethodTranslator(sqlExpressionFactory),
            new SnowflakeDateTimeMethodTranslator(sqlExpressionFactory, typeMappingSource),
            new SnowflakeFromPartsFunctionTranslator(sqlExpressionFactory, typeMappingSource),
            new SnowflakeFullTextSearchFunctionsTranslator(sqlExpressionFactory),
            new SnowflakeIsDateFunctionTranslator(sqlExpressionFactory),
            new SnowflakeIsNumericFunctionTranslator(sqlExpressionFactory),
            new SnowflakeMathTranslator(sqlExpressionFactory),
            new SnowflakeNewGuidTranslator(sqlExpressionFactory),
            new SnowflakeObjectToStringTranslator(sqlExpressionFactory),
            new SnowflakeStringMethodTranslator(sqlExpressionFactory),
            new SnowflakeTimeOnlyMethodTranslator(sqlExpressionFactory),
            new SnowflakeRegexIsMatchTranslator(sqlExpressionFactory)
        ]);
    }
}