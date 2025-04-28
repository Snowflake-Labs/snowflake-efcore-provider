using Snowflake.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Storage.Internal;

namespace Snowflake.EntityFrameworkCore.ValueGeneration.Internal;

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;

public class SnowflakeValueGeneratorSelector : RelationalValueGeneratorSelector
{
    private readonly ISnowflakeSequenceValueGeneratorFactory _sequenceFactory;
    private readonly ISnowflakeConnection _connection;
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
    private readonly IRelationalCommandDiagnosticsLogger _commandLogger;

    public  SnowflakeValueGeneratorSelector(
        ValueGeneratorSelectorDependencies dependencies,
        ISnowflakeSequenceValueGeneratorFactory sequenceFactory,
        ISnowflakeConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IRelationalCommandDiagnosticsLogger commandLogger)
        : base(dependencies)
    {
        _sequenceFactory = sequenceFactory;
        _connection = connection;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
        _commandLogger = commandLogger;
    }

    public  new virtual ISnowflakeValueGeneratorCache Cache
        => (ISnowflakeValueGeneratorCache)base.Cache;

    /// <inheritdoc />
    public override  ValueGenerator Select(IProperty property, ITypeBase typeBase)
    {
        if (property.GetValueGeneratorFactory() != null
            || property.GetValueGenerationStrategy() != SnowflakeValueGenerationStrategy.SequenceHiLo)
        {
            return base.Select(property, typeBase);
        }

        var propertyType = property.ClrType.UnwrapNullableType().UnwrapEnumType();

        var generator = _sequenceFactory.TryCreate(
            property,
            propertyType,
            Cache.GetOrAddSequenceState(property, _connection),
            _connection,
            _rawSqlCommandBuilder,
            _commandLogger);

        if (generator != null)
        {
            return generator;
        }

        var converter = property.GetTypeMapping().Converter;
        if (converter != null
            && converter.ProviderClrType != propertyType)
        {
            generator = _sequenceFactory.TryCreate(
                property,
                converter.ProviderClrType,
                Cache.GetOrAddSequenceState(property, _connection),
                _connection,
                _rawSqlCommandBuilder,
                _commandLogger);

            if (generator != null)
            {
                return generator.WithConverter(converter);
            }
        }

        throw new ArgumentException(
            CoreStrings.InvalidValueGeneratorFactoryProperty(
                nameof(SnowflakeSequenceValueGeneratorFactory), property.Name, property.DeclaringType.DisplayName()));
    }

    /// <inheritdoc />
    protected override  ValueGenerator? FindForType(IProperty property, ITypeBase typeBase, Type clrType)
        => property.ClrType.UnwrapNullableType() == typeof(Guid)
            ? property.ValueGenerated == ValueGenerated.Never || property.GetDefaultValueSql() != null
                ? new TemporaryGuidValueGenerator()
                : new SequentialGuidValueGenerator()
            : base.FindForType(property, typeBase, clrType);
}
