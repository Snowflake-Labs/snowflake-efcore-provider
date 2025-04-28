using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Snowflake.EntityFrameworkCore.Diagnostics.Internal;

public class SnowflakeLoggingDefinitions : RelationalLoggingDefinitions
{
    public EventDefinitionBase? LogStandardTable;
    
    public EventDefinitionBase? LogStandardTableFoundPrimaryKey;
    
    public EventDefinitionBase? LogStandardTableFoundForeignKey;

    /// <summary>
    ///     This is an event to warn about Migration Operation option being ignored.
    /// </summary>
    public EventDefinitionBase? LogMigrationOperationOptionNotSupported;

    /// <summary>
    ///     This is an event to warn about Sensitive Data Logging being enabled
    ///     on a Production environment (or not setup explicitly). 
    /// </summary>
    public EventDefinitionBase? LogSensitiveDataOnNonDevelopmentEnvironment;

    
    public EventDefinitionBase? LogDecimalTypeKey;

    
    public EventDefinitionBase? LogDefaultDecimalTypeColumn;

    
    public EventDefinitionBase? LogByteIdentityColumn;

    
    public EventDefinitionBase? LogColumnWithoutType;

    
    public EventDefinitionBase? LogFoundDefaultSchema;

    
    public EventDefinitionBase? LogFoundTypeAlias;

    
    public EventDefinitionBase? LogFoundColumn;

    
    public EventDefinitionBase? LogFoundForeignKey;

    
    public EventDefinitionBase? LogPrincipalTableNotInSelectionSet;

    
    public EventDefinitionBase? LogMissingSchema;

    public  EventDefinitionBase? LogMissingTable;

    public  EventDefinitionBase? LogFoundSequence;

    public  EventDefinitionBase? LogFoundTable;

    public  EventDefinitionBase? LogFoundIndex;

    public  EventDefinitionBase? LogFoundPrimaryKey;

    public  EventDefinitionBase? LogFoundUniqueConstraint;

    public  EventDefinitionBase? LogPrincipalColumnNotFound;

    public  EventDefinitionBase? LogReflexiveConstraintIgnored;

    public  EventDefinitionBase? LogDuplicateForeignKeyConstraintIgnored;

    public  EventDefinitionBase? LogPrincipalTableInformationNotFound;

    public  EventDefinitionBase? LogSavepointsDisabledBecauseOfMARS;

    public  EventDefinitionBase? LogConflictingValueGenerationStrategies;

    public  EventDefinitionBase? LogMissingViewDefinitionRights;
}
