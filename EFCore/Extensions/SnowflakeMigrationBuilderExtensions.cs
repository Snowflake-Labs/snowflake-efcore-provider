using Snowflake.EntityFrameworkCore.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations;

using System;

/// <summary>
///     Snowflake specific extension methods for <see cref="MigrationBuilder" />.
/// </summary>
public static class SnowflakeMigrationBuilderExtensions
{
    /// <summary>
    ///     Returns <see langword="true" /> if the database provider currently in use is the Snowflake provider.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="migrationBuilder">
    ///     The migrationBuilder from the parameters on <see cref="Migration.Up(MigrationBuilder)" /> or
    ///     <see cref="Migration.Down(MigrationBuilder)" />.
    /// </param>
    /// <returns><see langword="true" /> if Snowflake is being used; <see langword="false" /> otherwise.</returns>
    public static bool IsSnowflake(this MigrationBuilder migrationBuilder)
        => string.Equals(
            migrationBuilder.ActiveProvider,
            typeof(SnowflakeOptionsExtension).Assembly.GetName().Name,
            StringComparison.Ordinal);
}
