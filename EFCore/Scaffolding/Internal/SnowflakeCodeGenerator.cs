using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Snowflake.EntityFrameworkCore.Infrastructure;

namespace Snowflake.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///    A <see cref="ProviderCodeGenerator" /> for Snowflake.
/// </summary>
public class SnowflakeCodeGenerator : ProviderCodeGenerator
{
    private static readonly MethodInfo UseSnowflakeMethodInfo
        = typeof(SnowflakeDbContextOptionsExtensions).GetRuntimeMethod(
            nameof(SnowflakeDbContextOptionsExtensions.UseSnowflake),
            [typeof(DbContextOptionsBuilder), typeof(string), typeof(Action<SnowflakeDbContextOptionsBuilder>)])!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SnowflakeCodeGenerator" /> class.
    /// </summary>
    /// <param name="dependencies">The dependencies.</param>
    public SnowflakeCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override MethodCallCodeFragment GenerateUseProvider(
        string connectionString,
        MethodCallCodeFragment? providerOptions)
        => new(
            UseSnowflakeMethodInfo,
            providerOptions == null
                ? [connectionString]
                : [connectionString, new NestedClosureCodeFragment("x", providerOptions)]);
}