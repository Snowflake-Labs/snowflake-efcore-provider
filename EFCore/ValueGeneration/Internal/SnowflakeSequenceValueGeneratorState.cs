namespace Snowflake.EntityFrameworkCore.ValueGeneration.Internal;

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

public class SnowflakeSequenceValueGeneratorState : HiLoValueGeneratorState
{
    public  SnowflakeSequenceValueGeneratorState(ISequence sequence)
        : base(sequence.IncrementBy)
    {
        Sequence = sequence;
    }

    public  virtual ISequence Sequence { get; }
}
