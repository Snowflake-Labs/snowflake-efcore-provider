using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates <see cref="object.ToString" /> calls into Snowflake CAST expressions.
/// </summary>
public class SnowflakeObjectToStringTranslator : IMethodCallTranslator
{
    private const int DefaultLength = 100;

    private static readonly Dictionary<Type, string> TypeMapping
        = new()
        {
            { typeof(sbyte), "varchar(4)" },
            { typeof(byte), "varchar(3)" },
            { typeof(short), "varchar(6)" },
            { typeof(ushort), "varchar(5)" },
            { typeof(int), "varchar(11)" },
            { typeof(uint), "varchar(10)" },
            { typeof(long), "varchar(20)" },
            { typeof(ulong), "varchar(20)" },
            { typeof(float), $"varchar({DefaultLength})" },
            { typeof(double), $"varchar({DefaultLength})" },
            { typeof(decimal), $"varchar({DefaultLength})" },
            { typeof(char), "varchar(1)" },
            { typeof(DateTime), $"varchar({DefaultLength})" },
            { typeof(DateOnly), $"varchar({DefaultLength})" },
            { typeof(TimeOnly), $"varchar({DefaultLength})" },
            { typeof(DateTimeOffset), $"varchar({DefaultLength})" },
            { typeof(TimeSpan), $"varchar({DefaultLength})" },
            { typeof(Guid), "varchar(36)" },
            { typeof(byte[]), $"varchar({DefaultLength})" }
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///    Creates a new instance of the <see cref="SnowflakeObjectToStringTranslator" /> class.
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeObjectToStringTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance == null || method.Name != nameof(ToString) || arguments.Count != 0)
        {
            return null;
        }

        if (instance.TypeMapping?.ClrType == typeof(string))
        {
            return instance;
        }

        if (instance.Type == typeof(bool))
        {
            if (instance is ColumnExpression { IsNullable: true })
            {
                return _sqlExpressionFactory.Case(
                    new[]
                    {
                        new CaseWhenClause(
                            _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(false)),
                            _sqlExpressionFactory.Constant(false.ToString())),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(true)),
                            _sqlExpressionFactory.Constant(true.ToString()))
                    },
                    _sqlExpressionFactory.Constant(null));
            }

            return _sqlExpressionFactory.Case(
                new[]
                {
                    new CaseWhenClause(
                        _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(false)),
                        _sqlExpressionFactory.Constant(false.ToString()))
                },
                _sqlExpressionFactory.Constant(true.ToString()));
        }

        return TypeMapping.TryGetValue(instance.Type, out var storeType)
            ? new SnowflakeCastFunctionExpression(
                instance,
                _sqlExpressionFactory.Fragment(storeType),
                instance.Type,
                instance.TypeMapping)
            : null;
    }
}