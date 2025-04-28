namespace Snowflake.EntityFrameworkCore.Constants;

/// <summary>
/// GS error code mapping taken from https://github.com/snowflakedb/snowflake/tree/main/GlobalServices/modules/platform/core/src/main/java/com/snowflake/common/errorcodes
/// </summary>
public static class SnowflakeErrorCodes
{
    /// <summary>
    /// Actual statement count {0} did not match the desired statement count {1}.
    /// </summary>
    public const int StatementCountNotMatch = 000008;
}