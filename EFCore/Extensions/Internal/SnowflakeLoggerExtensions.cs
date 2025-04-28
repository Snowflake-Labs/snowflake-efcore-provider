using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Snowflake.EntityFrameworkCore.Diagnostics;
using Snowflake.EntityFrameworkCore.Internal;
using Snowflake.EntityFrameworkCore.Metadata;

namespace Snowflake.EntityFrameworkCore.Extensions.Internal;

public static class SnowflakeLoggerExtensions
{
    public static void StandardTableWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IEntityType entityType)
    {
        var definition = SnowflakeResources.LogStandardTable(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, entityType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EntityTypeEventData(
                definition,
                StandardTableWarning,
                entityType);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string StandardTableWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (EntityTypeEventData)payload;
        return d.GenerateMessage(
            p.EntityType.DisplayName());
    }
    
    public static void StandardTableFoundPrimaryKey(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string primaryKeyName,
        string tableName)
    {
        var definition = SnowflakeResources.LogStandardTableFoundPrimaryKey(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, primaryKeyName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
    
    public static void MigrationOperationOptionNotSupported(
        this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
        string option,
        string operation)
    {
        var definition = SnowflakeResources.LogMigrationOperationOptionNotSupported(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, option, operation);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
    
    public static void StandardTableForeignKeyFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string foreignKeyName,
        string tableName,
        string principalTableName)
    {
        var definition = SnowflakeResources.LogStandardTableFoundForeignKey(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, foreignKeyName, tableName, principalTableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
    
    
    public static void SensitiveDataOnNonDevelopmentEnvironmentError(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics, EnvironmentType environmentType)
    {
        var definition = SnowflakeResources.LogSensitiveDataLoggingOnNonDevelopmentEnvironment(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, environmentType.ToString());
        }
        
        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EventData(
                definition,
                SensitiveDataOnNonDevelopmentEnvironmentError);
        
            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string SensitiveDataOnNonDevelopmentEnvironmentError(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        return d.GenerateMessage(payload.ToString());
    }
    
    public static void DecimalTypeKeyWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IProperty property)
    {
        var definition = SnowflakeResources.LogDecimalTypeKey(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, property.Name, property.DeclaringType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyEventData(
                definition,
                DecimalTypeKeyWarning,
                property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string DecimalTypeKeyWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (PropertyEventData)payload;
        return d.GenerateMessage(
            p.Property.Name,
            p.Property.DeclaringType.DisplayName());
    }
    
    public static void DecimalTypeDefaultWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IProperty property)
    {
        var definition = SnowflakeResources.LogDefaultDecimalTypeColumn(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, property.Name, property.DeclaringType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyEventData(
                definition,
                DecimalTypeDefaultWarning,
                property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string DecimalTypeDefaultWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (PropertyEventData)payload;
        return d.GenerateMessage(
            p.Property.Name,
            p.Property.DeclaringType.DisplayName());
    }
    
    public static void ByteIdentityColumnWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IProperty property)
    {
        var definition = SnowflakeResources.LogByteIdentityColumn(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, property.Name, property.DeclaringType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyEventData(
                definition,
                ByteIdentityColumnWarning,
                property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ByteIdentityColumnWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (PropertyEventData)payload;
        return d.GenerateMessage(
            p.Property.Name,
            p.Property.DeclaringType.DisplayName());
    }
    
    public static void ConflictingValueGenerationStrategiesWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        SnowflakeValueGenerationStrategy SnowflakeValueGenerationStrategy,
        string otherValueGenerationStrategy,
        IReadOnlyProperty property)
    {
        var definition = SnowflakeResources.LogConflictingValueGenerationStrategies(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics, SnowflakeValueGenerationStrategy.ToString(), otherValueGenerationStrategy,
                property.Name, property.DeclaringType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ConflictingValueGenerationStrategiesEventData(
                definition,
                ConflictingValueGenerationStrategiesWarning,
                SnowflakeValueGenerationStrategy,
                otherValueGenerationStrategy,
                property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ConflictingValueGenerationStrategiesWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string, string>)definition;
        var p = (ConflictingValueGenerationStrategiesEventData)payload;
        return d.GenerateMessage(
            p.SnowflakeValueGenerationStrategy.ToString(),
            p.OtherValueGenerationStrategy,
            p.Property.Name,
            p.Property.DeclaringType.DisplayName());
    }
    
    public static void ColumnFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string tableName,
        string columnName,
        long ordinal,
        string dataTypeName,
        long maxLength,
        long precision,
        long scale,
        bool nullable,
        bool identity,
        string? defaultValue)
    {
        var definition = SnowflakeResources.LogFoundColumn(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                l => l.LogDebug(
                    definition.EventId,
                    null,
                    definition.MessageFormat,
                    tableName,
                    columnName,
                    ordinal,
                    dataTypeName,
                    maxLength,
                    precision,
                    scale,
                    nullable,
                    identity,
                    defaultValue));
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
    
    public static void ForeignKeyFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string foreignKeyName,
        string tableName,
        string principalTableName,
        string onDeleteAction)
    {
        var definition = SnowflakeResources.LogFoundForeignKey(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, foreignKeyName, tableName, principalTableName, onDeleteAction);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
    
    public static void DefaultSchemaFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string schemaName)
    {
        var definition = SnowflakeResources.LogFoundDefaultSchema(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, schemaName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
    public static void PrimaryKeyFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string primaryKeyName,
        string tableName)
    {
        var definition = SnowflakeResources.LogFoundPrimaryKey(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, primaryKeyName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
    
    public static void UniqueConstraintFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string uniqueConstraintName,
        string tableName)
    {
        var definition = SnowflakeResources.LogFoundUniqueConstraint(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, uniqueConstraintName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
    
    public static void IndexFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string indexName,
        string tableName,
        bool unique)
    {
        var definition = SnowflakeResources.LogFoundIndex(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, indexName, tableName, unique);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
    
    public static void ForeignKeyReferencesUnknownPrincipalTableWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? foreignKeyName,
        string? tableName)
    {
        var definition = SnowflakeResources.LogPrincipalTableInformationNotFound(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, foreignKeyName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    
    public static void ForeignKeyReferencesMissingPrincipalTableWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? foreignKeyName,
        string? tableName,
        string? principalTableName)
    {
        var definition = SnowflakeResources.LogPrincipalTableNotInSelectionSet(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, foreignKeyName, tableName, principalTableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    
    public static void ForeignKeyPrincipalColumnMissingWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string foreignKeyName,
        string tableName,
        string principalColumnName,
        string principalTableName)
    {
        var definition = SnowflakeResources.LogPrincipalColumnNotFound(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, foreignKeyName, tableName, principalColumnName, principalTableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    
    public static void MissingSchemaWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? schemaName)
    {
        var definition = SnowflakeResources.LogMissingSchema(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, schemaName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    
    public static void MissingTableWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? tableName)
    {
        var definition = SnowflakeResources.LogMissingTable(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    
    public static void ColumnWithoutTypeWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string tableName,
        string columnName)
    {
        var definition = SnowflakeResources.LogColumnWithoutType(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, tableName, columnName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    
    public static void SequenceFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string sequenceName,
        string sequenceTypeName,
        int increment,
        long start,
        long min,
        long max)
    {
        // No DiagnosticsSource events because these are purely design-time messages
        var definition = SnowflakeResources.LogFoundSequence(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                l => l.LogDebug(
                    definition.EventId,
                    null,
                    definition.MessageFormat,
                    sequenceName,
                    sequenceTypeName,
                    increment,
                    start,
                    min,
                    max));
        }
    }

    public  static void TableFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string tableName)
    {
        var definition = SnowflakeResources.LogFoundTable(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public  static void ReflexiveConstraintIgnored(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string foreignKeyName,
        string tableName)
    {
        var definition = SnowflakeResources.LogReflexiveConstraintIgnored(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, foreignKeyName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public  static void DuplicateForeignKeyConstraintIgnored(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string foreignKeyName,
        string tableName,
        string duplicateForeignKeyName)
    {
        var definition = SnowflakeResources.LogDuplicateForeignKeyConstraintIgnored(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, foreignKeyName, tableName, duplicateForeignKeyName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public  static void SavepointsDisabledBecauseOfMARS(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics)
    {
        var definition = SnowflakeResources.LogSavepointsDisabledBecauseOfMARS(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EventData(
                definition,
                (d, _) => ((EventDefinition)d).GenerateMessage());

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    public  static void MissingViewDefinitionRightsWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics)
    {
        var definition = SnowflakeResources.LogMissingViewDefinitionRights(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
}
