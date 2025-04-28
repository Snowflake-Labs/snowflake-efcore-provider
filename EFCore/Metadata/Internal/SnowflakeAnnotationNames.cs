namespace Snowflake.EntityFrameworkCore.Metadata.Internal;

/// <summary>
/// Provides the names of annotations used by Snowflake.
/// </summary>
public static class SnowflakeAnnotationNames
{
    /// <summary>
    /// Base prefix for all Snowflake annotations.
    /// </summary>
    public const string Prefix = "Snowflake:";

    /// <summary>
    /// Default name of the sequence used for the HiLo pattern.
    /// </summary>
    public const string HiLoSequenceName = Prefix + "HiLoSequenceName";

    /// <summary>
    /// Default schema of the sequence used for the HiLo pattern.
    /// </summary>
    public const string HiLoSequenceSchema = Prefix + "HiLoSequenceSchema";

    /// <summary>
    /// Default name of the sequences.
    /// </summary>
    public const string SequenceNameSuffix = Prefix + "SequenceNameSuffix";

    /// <summary>
    /// Default schema name.
    /// </summary>
    public const string SequenceName = Prefix + "SequenceName";

    /// <summary>
    /// Sequence schema name.
    /// </summary>
    public const string SequenceSchema = Prefix + "SequenceSchema";

    /// <summary>
    /// Identity name.
    /// </summary>
    public const string Identity = Prefix + "Identity";

    /// <summary>
    /// Identity increment name.
    /// </summary>
    public const string IdentityIncrement = Prefix + "IdentityIncrement";

    /// <summary>
    /// Identity seed name.
    /// </summary>
    public const string IdentitySeed = Prefix + "IdentitySeed";

    /// <summary>
    /// Include name.
    /// </summary>
    public const string Include = Prefix + "Include";

    /// <summary>
    /// IsTemporal annotation name.
    /// </summary>
    public const string IsTemporal = Prefix + "IsTemporal";

    /// <summary>
    /// Value generation strategy name.
    /// </summary>
    public const string ValueGenerationStrategy = Prefix + "ValueGenerationStrategy";

    /// <summary>
    /// Indicates that the entity is associated to a Hybrid Table.
    /// </summary>
    public const string HybridTable = Prefix + "HybridTable";
}