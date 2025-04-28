using Snowflake.EntityFrameworkCore.Storage.Internal;

namespace Snowflake.EntityFrameworkCore.ValueGeneration.Internal;

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;

public interface ISnowflakeSequenceValueGeneratorFactory
{
    ValueGenerator? TryCreate(
        IProperty property,
        Type clrType,
        SnowflakeSequenceValueGeneratorState generatorState,
        ISnowflakeConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IRelationalCommandDiagnosticsLogger commandLogger);
}
