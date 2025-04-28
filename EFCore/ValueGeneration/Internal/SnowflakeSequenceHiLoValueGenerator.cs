using System.Globalization;
using Snowflake.EntityFrameworkCore.Storage.Internal;
using Snowflake.EntityFrameworkCore.Update.Internal;

namespace Snowflake.EntityFrameworkCore.ValueGeneration.Internal;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;

public class SnowflakeSequenceHiLoValueGenerator<TValue> : HiLoValueGenerator<TValue>
{
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
    private readonly ISnowflakeUpdateSqlGenerator _sqlGenerator;
    private readonly ISnowflakeConnection _connection;
    private readonly ISequence _sequence;
    private readonly IRelationalCommandDiagnosticsLogger _commandLogger;

    public  SnowflakeSequenceHiLoValueGenerator(
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        ISnowflakeUpdateSqlGenerator sqlGenerator,
        SnowflakeSequenceValueGeneratorState generatorState,
        ISnowflakeConnection connection,
        IRelationalCommandDiagnosticsLogger commandLogger)
        : base(generatorState)
    {
        _sequence = generatorState.Sequence;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
        _sqlGenerator = sqlGenerator;
        _connection = connection;
        _commandLogger = commandLogger;
    }

    /// <inheritdoc />
    protected override  long GetNewLowValue()
        => (long)Convert.ChangeType(
            _rawSqlCommandBuilder
                .Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
                .ExecuteScalar(
                    new RelationalCommandParameterObject(
                        _connection,
                        parameterValues: null,
                        readerColumns: null,
                        context: null,
                        _commandLogger, CommandSource.ValueGenerator)),
            typeof(long),
            CultureInfo.InvariantCulture)!;

    /// <inheritdoc />
    protected override  async Task<long> GetNewLowValueAsync(CancellationToken cancellationToken = default)
        => (long)Convert.ChangeType(
            await _rawSqlCommandBuilder
                .Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
                .ExecuteScalarAsync(
                    new RelationalCommandParameterObject(
                        _connection,
                        parameterValues: null,
                        readerColumns: null,
                        context: null,
                        _commandLogger, CommandSource.ValueGenerator),
                    cancellationToken)
                .ConfigureAwait(false),
            typeof(long),
            CultureInfo.InvariantCulture)!;

    /// <inheritdoc />
    public override  bool GeneratesTemporaryValues
        => false;
}
