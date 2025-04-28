namespace Snowflake.EntityFrameworkCore.Update.Internal;

using Microsoft.EntityFrameworkCore.Update;

/// <summary>
///  Represents a Snowflake-specific modification command. 
/// </summary>
public class SnowflakeModificationCommand : ModificationCommand
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="SnowflakeModificationCommand" /> class.
    /// </summary>
    /// <param name="modificationCommandParameters"></param>
    public SnowflakeModificationCommand(in ModificationCommandParameters modificationCommandParameters)
        : base(modificationCommandParameters)
    {
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="SnowflakeModificationCommand" /> class.
    /// </summary>
    /// <param name="modificationCommandParameters"></param>
    public SnowflakeModificationCommand(in NonTrackedModificationCommandParameters modificationCommandParameters)
        : base(modificationCommandParameters)
    {
    }
}