using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Metadata;

namespace Snowflake.EntityFrameworkCore.Diagnostics;


/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that have
///     a property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class ConflictingValueGenerationStrategiesEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="SnowflakeValueGenerationStrategy">The Snowflake value generation strategy.</param>
    /// <param name="otherValueGenerationStrategy">The other value generation strategy.</param>
    /// <param name="property">The property.</param>
    public ConflictingValueGenerationStrategiesEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        SnowflakeValueGenerationStrategy SnowflakeValueGenerationStrategy,
        string otherValueGenerationStrategy,
        IReadOnlyProperty property)
        : base(eventDefinition, messageGenerator)
    {
        SnowflakeValueGenerationStrategy = SnowflakeValueGenerationStrategy;
        OtherValueGenerationStrategy = otherValueGenerationStrategy;
        Property = property;
    }

    /// <summary>
    ///     The Snowflake value generation strategy.
    /// </summary>
    public virtual SnowflakeValueGenerationStrategy SnowflakeValueGenerationStrategy { get; }

    /// <summary>
    ///     The other value generation strategy.
    /// </summary>
    public virtual string OtherValueGenerationStrategy { get; }

    /// <summary>
    ///     The property.
    /// </summary>
    public virtual IReadOnlyProperty Property { get; }
}
