using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates <see cref="Guid.NewGuid"/> method calls into <c>UUID_STRING()</c> function calls.
/// </summary>
public class SnowflakeNewGuidTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo MethodInfo = typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), Type.EmptyTypes)!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///    Creates a new instance of the <see cref="SnowflakeNewGuidTranslator" /> class.
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeNewGuidTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => MethodInfo.Equals(method)
            ? _sqlExpressionFactory.Function(
                "UUID_STRING",
                [],
                nullable: false,
                argumentsPropagateNullability: [],
                method.ReturnType)
            : null;
}