namespace Snowflake.EntityFrameworkCore.Extensions;

public enum EnvironmentType
{
    /// <summary>
    ///   Production environment value is set with SnowflakeDbContextOptionsExtensions.UseEnvironment method
    ///   to indicate that the code is running on Production environment.
    /// </summary>
    Production,
    /// <summary>
    ///   Development environment value is set with SnowflakeDbContextOptionsExtensions.UseEnvironment method
    ///   to indicate that code is running on non-production environment.
    /// </summary>
    Development,
    /// <summary>
    ///   Unknown value of environment is used when SnowflakeDbContextOptionsExtensions.UseEnvironment method
    ///   has not been called.
    /// </summary>
    Unknown
}
