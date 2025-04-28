using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Snowflake.EntityFrameworkCore.Extensions.Internal;
using Snowflake.EntityFrameworkCore.Internal;
using Snowflake.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Metadata.Internal;

namespace Snowflake.EntityFrameworkCore.Infrastructure.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

public class SnowflakeModelValidator : RelationalModelValidator
{
    
    public SnowflakeModelValidator(
        ModelValidatorDependencies dependencies,
        RelationalModelValidatorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <inheritdoc />
    public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        ValidateIndexIncludeProperties(model, logger);

        base.Validate(model, logger);
        ValidateStandardTables(model, logger);
        ValidateDecimalColumns(model, logger);
        ValidateByteIdentityMapping(model, logger);
        ValidateTemporalTables(model, logger);
    }

    protected  virtual void ValidateStandardTables(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            if (!entityType.IsHybridTable())
                logger.StandardTableWarning(entityType);
        }
    }

    protected  virtual void ValidateDecimalColumns(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (IConventionProperty property in model.GetEntityTypes()
                     .SelectMany(t => t.GetDeclaredProperties())
                     .Where(
                         p => p.ClrType.UnwrapNullableType() == typeof(decimal)
                             && !p.IsForeignKey()))
        {
            var valueConverterConfigurationSource = property.GetValueConverterConfigurationSource();
            var valueConverterProviderType = property.GetValueConverter()?.ProviderClrType;
            if (!ConfigurationSource.Convention.Overrides(valueConverterConfigurationSource)
                && typeof(decimal) != valueConverterProviderType)
            {
                continue;
            }

            var columnTypeConfigurationSource = property.GetColumnTypeConfigurationSource();
            if (((columnTypeConfigurationSource == null
                        && ConfigurationSource.Convention.Overrides(property.GetTypeMappingConfigurationSource()))
                    || (columnTypeConfigurationSource != null
                        && ConfigurationSource.Convention.Overrides(columnTypeConfigurationSource)))
                && (ConfigurationSource.Convention.Overrides(property.GetPrecisionConfigurationSource())
                    || ConfigurationSource.Convention.Overrides(property.GetScaleConfigurationSource())))
            {
                logger.DecimalTypeDefaultWarning((IProperty)property);
            }

            if (property.IsKey())
            {
                logger.DecimalTypeKeyWarning((IProperty)property);
            }
        }
    }

    protected  virtual void ValidateByteIdentityMapping(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            // TODO: Validate this per table
            foreach (var property in entityType.GetDeclaredProperties()
                         .Where(
                             p => p.ClrType.UnwrapNullableType() == typeof(byte)
                                 && p.GetValueGenerationStrategy() == SnowflakeValueGenerationStrategy.IdentityColumn))
            {
                logger.ByteIdentityColumnWarning(property);
            }
        }
    }

    /// <inheritdoc />
    protected override  void ValidateValueGeneration(
        IEntityType entityType,
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (entityType.GetTableName() != null
            && (string?)entityType[RelationalAnnotationNames.MappingStrategy] == RelationalAnnotationNames.TpcMappingStrategy)
        {
            foreach (var storeGeneratedProperty in key.Properties.Where(
                         p => (p.ValueGenerated & ValueGenerated.OnAdd) != 0
                             && p.GetValueGenerationStrategy() == SnowflakeValueGenerationStrategy.IdentityColumn))
            {
                logger.TpcStoreGeneratedIdentityWarning(storeGeneratedProperty);
            }
        }
    }

    protected  virtual void ValidateIndexIncludeProperties(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var index in model.GetEntityTypes().SelectMany(t => t.GetDeclaredIndexes()))
        {
            var includeProperties = index.GetIncludeProperties();
            if (includeProperties?.Count > 0)
            {
                var notFound = includeProperties
                    .FirstOrDefault(i => index.DeclaringEntityType.FindProperty(i) == null);

                if (notFound != null)
                {
                    throw new InvalidOperationException(
                        SnowflakeStrings.IncludePropertyNotFound(
                            notFound,
                            index.DisplayName(),
                            index.DeclaringEntityType.DisplayName()));
                }

                var duplicateProperty = includeProperties
                    .GroupBy(i => i)
                    .Where(g => g.Count() > 1)
                    .Select(y => y.Key)
                    .FirstOrDefault();

                if (duplicateProperty != null)
                {
                    throw new InvalidOperationException(
                        SnowflakeStrings.IncludePropertyDuplicated(
                            index.DeclaringEntityType.DisplayName(),
                            duplicateProperty,
                            index.DisplayName()));
                }

                var coveredProperty = includeProperties
                    .FirstOrDefault(i => index.Properties.Any(p => i == p.Name));

                if (coveredProperty != null)
                {
                    throw new InvalidOperationException(
                        SnowflakeStrings.IncludePropertyInIndex(
                            index.DeclaringEntityType.DisplayName(),
                            coveredProperty,
                            index.DisplayName()));
                }
            }
        }
    }

    protected  virtual void ValidateTemporalTables(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var temporalEntityTypes = model.GetEntityTypes().Where(t => t.IsTemporal()).ToList();
        foreach (var temporalEntityType in temporalEntityTypes)
        {
            if (temporalEntityType.BaseType != null)
            {
                throw new InvalidOperationException(SnowflakeStrings.TemporalOnlyOnRoot(temporalEntityType.DisplayName()));
            }

            var derivedTableMappings = temporalEntityType.GetDerivedTypes().Select(t => t.GetTableName()).Distinct().ToList();
            if (derivedTableMappings.Count > 0
                && (derivedTableMappings.Count != 1 || derivedTableMappings.First() != temporalEntityType.GetTableName()))
            {
                throw new InvalidOperationException(SnowflakeStrings.TemporalOnlySupportedForTPH(temporalEntityType.DisplayName()));
            }
        }
    }

    /// <inheritdoc />
    protected override  void ValidateSharedTableCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (mappedTypes.Any(t => t.IsTemporal())
            && mappedTypes.Select(t => t.GetRootType()).Distinct().Count() > 1)
        {
            throw new InvalidOperationException(SnowflakeStrings.TemporalNotSupportedForTableSplitting(storeObject.DisplayName()));
        }

        base.ValidateSharedTableCompatibility(mappedTypes, storeObject, logger);
    }

    /// <inheritdoc />
    protected override  void ValidateSharedColumnsCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateSharedColumnsCompatibility(mappedTypes, storeObject, logger);

        var identityColumns = new Dictionary<string, IProperty>();

        foreach (var property in mappedTypes.SelectMany(et => et.GetDeclaredProperties()))
        {
            var columnName = property.GetColumnName(storeObject);
            if (columnName == null)
            {
                continue;
            }

            if (property.GetValueGenerationStrategy(storeObject) == SnowflakeValueGenerationStrategy.IdentityColumn)
            {
                identityColumns[columnName] = property;
            }
        }

        if (identityColumns.Count > 1)
        {
            var sb = new StringBuilder()
                .AppendJoin(identityColumns.Values.Select(p => "'" + p.DeclaringType.DisplayName() + "." + p.Name + "'"));
            throw new InvalidOperationException(SnowflakeStrings.MultipleIdentityColumns(sb, storeObject.DisplayName()));
        }
    }

    /// <inheritdoc />
    protected override void ValidateCompatible(
        IProperty property,
        IProperty duplicateProperty,
        string columnName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateCompatible(property, duplicateProperty, columnName, storeObject, logger);

        var propertyStrategy = property.GetValueGenerationStrategy(storeObject);
        var duplicatePropertyStrategy = duplicateProperty.GetValueGenerationStrategy(storeObject);
        if (propertyStrategy != duplicatePropertyStrategy)
        {
            var isConflicting = ((IConventionProperty)property)
                .FindAnnotation(SnowflakeAnnotationNames.ValueGenerationStrategy)
                ?.GetConfigurationSource()
                == ConfigurationSource.Explicit
                || propertyStrategy != SnowflakeValueGenerationStrategy.None;
            var isDuplicateConflicting = ((IConventionProperty)duplicateProperty)
                .FindAnnotation(SnowflakeAnnotationNames.ValueGenerationStrategy)
                ?.GetConfigurationSource()
                == ConfigurationSource.Explicit
                || duplicatePropertyStrategy != SnowflakeValueGenerationStrategy.None;

            if (isConflicting && isDuplicateConflicting)
            {
                throw new InvalidOperationException(
                    SnowflakeStrings.DuplicateColumnNameValueGenerationStrategyMismatch(
                        duplicateProperty.DeclaringType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName()));
            }
        }
        else
        {
            switch (propertyStrategy)
            {
                case SnowflakeValueGenerationStrategy.IdentityColumn:
                    var increment = property.GetIdentityIncrement(storeObject);
                    var duplicateIncrement = duplicateProperty.GetIdentityIncrement(storeObject);
                    if (increment != duplicateIncrement)
                    {
                        throw new InvalidOperationException(
                            SnowflakeStrings.DuplicateColumnIdentityIncrementMismatch(
                                duplicateProperty.DeclaringType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringType.DisplayName(),
                                property.Name,
                                columnName,
                                storeObject.DisplayName()));
                    }

                    var seed = property.GetIdentitySeed(storeObject);
                    var duplicateSeed = duplicateProperty.GetIdentitySeed(storeObject);
                    if (seed != duplicateSeed)
                    {
                        throw new InvalidOperationException(
                            SnowflakeStrings.DuplicateColumnIdentitySeedMismatch(
                                duplicateProperty.DeclaringType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringType.DisplayName(),
                                property.Name,
                                columnName,
                                storeObject.DisplayName()));
                    }

                    break;
                case SnowflakeValueGenerationStrategy.SequenceHiLo:
                    if (property.GetHiLoSequenceName(storeObject) != duplicateProperty.GetHiLoSequenceName(storeObject)
                        || property.GetHiLoSequenceSchema(storeObject) != duplicateProperty.GetHiLoSequenceSchema(storeObject))
                    {
                        throw new InvalidOperationException(
                            SnowflakeStrings.DuplicateColumnSequenceMismatch(
                                duplicateProperty.DeclaringType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringType.DisplayName(),
                                property.Name,
                                columnName,
                                storeObject.DisplayName()));
                    }

                    break;
            }
        }
    }

    /// <inheritdoc />
    protected override void ValidateCompatible(
        IKey key,
        IKey duplicateKey,
        string keyName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateCompatible(key, duplicateKey, keyName, storeObject, logger);

        key.AreCompatibleForSnowflake(duplicateKey, storeObject, shouldThrow: true);
    }

    /// <inheritdoc />
    protected override void ValidateCompatible(
        IIndex index,
        IIndex duplicateIndex,
        string indexName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateCompatible(index, duplicateIndex, indexName, storeObject, logger);

        index.AreCompatibleForSnowflake(duplicateIndex, storeObject, shouldThrow: true);
    }
}
