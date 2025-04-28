using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Metadata.Internal;
using Snowflake.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Snowflake.EntityFrameworkCore;


/// <summary>
///     Model extension methods for Snowflake-specific metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SnowflakeModelExtensions
{
    /// <summary>
    ///     The default name for the hi-lo sequence.
    /// </summary>
    public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";

    /// <summary>
    ///     The default prefix for sequences applied to properties.
    /// </summary>
    public const string DefaultSequenceNameSuffix = "Sequence";

    /// <summary>
    ///     Returns the name to use for the default hi-lo sequence.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The name to use for the default hi-lo sequence.</returns>
    public static string GetHiLoSequenceName(this IReadOnlyModel model)
        => (string?)model[SnowflakeAnnotationNames.HiLoSequenceName]
            ?? DefaultHiLoSequenceName;

    /// <summary>
    ///     Sets the name to use for the default hi-lo sequence.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="name">The value to set.</param>
    public static void SetHiLoSequenceName(this IMutableModel model, string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        model.SetOrRemoveAnnotation(SnowflakeAnnotationNames.HiLoSequenceName, name);
    }

    /// <summary>
    ///     Sets the name to use for the default hi-lo sequence.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="name">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetHiLoSequenceName(
        this IConventionModel model,
        string? name,
        bool fromDataAnnotation = false)
        => (string?)model.SetOrRemoveAnnotation(
            SnowflakeAnnotationNames.HiLoSequenceName,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the default hi-lo sequence name.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the default hi-lo sequence name.</returns>
    public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(SnowflakeAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the schema to use for the default hi-lo sequence.
    ///     <see cref="SnowflakePropertyBuilderExtensions.UseHiLo" />
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The schema to use for the default hi-lo sequence.</returns>
    public static string? GetHiLoSequenceSchema(this IReadOnlyModel model)
        => (string?)model[SnowflakeAnnotationNames.HiLoSequenceSchema];

    /// <summary>
    ///     Sets the schema to use for the default hi-lo sequence.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="value">The value to set.</param>
    public static void SetHiLoSequenceSchema(this IMutableModel model, string? value)
    {
        Check.NullButNotEmpty(value, nameof(value));

        model.SetOrRemoveAnnotation(SnowflakeAnnotationNames.HiLoSequenceSchema, value);
    }

    /// <summary>
    ///     Sets the schema to use for the default hi-lo sequence.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetHiLoSequenceSchema(
        this IConventionModel model,
        string? value,
        bool fromDataAnnotation = false)
        => (string?)model.SetOrRemoveAnnotation(
            SnowflakeAnnotationNames.HiLoSequenceSchema,
            Check.NullButNotEmpty(value, nameof(value)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the default hi-lo sequence schema.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the default hi-lo sequence schema.</returns>
    public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(SnowflakeAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the suffix to append to the name of automatically created sequences.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The name to use for the default key value generation sequence.</returns>
    public static string GetSequenceNameSuffix(this IReadOnlyModel model)
        => (string?)model[SnowflakeAnnotationNames.SequenceNameSuffix]
            ?? DefaultSequenceNameSuffix;

    /// <summary>
    ///     Sets the suffix to append to the name of automatically created sequences.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="name">The value to set.</param>
    public static void SetSequenceNameSuffix(this IMutableModel model, string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        model.SetOrRemoveAnnotation(SnowflakeAnnotationNames.SequenceNameSuffix, name);
    }

    /// <summary>
    ///     Sets the suffix to append to the name of automatically created sequences.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="name">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetSequenceNameSuffix(
        this IConventionModel model,
        string? name,
        bool fromDataAnnotation = false)
        => (string?)model.SetOrRemoveAnnotation(
            SnowflakeAnnotationNames.SequenceNameSuffix,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the default value generation sequence name suffix.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the default key value generation sequence name.</returns>
    public static ConfigurationSource? GetSequenceNameSuffixConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(SnowflakeAnnotationNames.SequenceNameSuffix)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the schema to use for the default value generation sequence.
    ///     <see cref="SnowflakePropertyBuilderExtensions.UseSequence" />
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The schema to use for the default key value generation sequence.</returns>
    public static string? GetSequenceSchema(this IReadOnlyModel model)
        => (string?)model[SnowflakeAnnotationNames.SequenceSchema];

    /// <summary>
    ///     Sets the schema to use for the default key value generation sequence.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="value">The value to set.</param>
    public static void SetSequenceSchema(this IMutableModel model, string? value)
    {
        Check.NullButNotEmpty(value, nameof(value));

        model.SetOrRemoveAnnotation(SnowflakeAnnotationNames.SequenceSchema, value);
    }

    /// <summary>
    ///     Sets the schema to use for the default key value generation sequence.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetSequenceSchema(
        this IConventionModel model,
        string? value,
        bool fromDataAnnotation = false)
        => (string?)model.SetOrRemoveAnnotation(
            SnowflakeAnnotationNames.SequenceSchema,
            Check.NullButNotEmpty(value, nameof(value)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the default key value generation sequence schema.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the default key value generation sequence schema.</returns>
    public static ConfigurationSource? GetSequenceSchemaConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(SnowflakeAnnotationNames.SequenceSchema)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the default identity seed.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The default identity seed.</returns>
    public static long GetIdentitySeed(this IReadOnlyModel model)
    {
        if (model is RuntimeModel)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        // Support pre-6.0 IdentitySeed annotations, which contained an int rather than a long
        var annotation = model.FindAnnotation(SnowflakeAnnotationNames.IdentitySeed);
        return annotation is null || annotation.Value is null
            ? 1
            : annotation.Value is int intValue
                ? intValue
                : (long)annotation.Value;
    }

    /// <summary>
    ///     Sets the default identity seed.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="seed">The value to set.</param>
    public static void SetIdentitySeed(this IMutableModel model, long? seed)
        => model.SetOrRemoveAnnotation(
            SnowflakeAnnotationNames.IdentitySeed,
            seed);

    /// <summary>
    ///     Sets the default identity seed.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="seed">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static long? SetIdentitySeed(this IConventionModel model, long? seed, bool fromDataAnnotation = false)
        => (long?)model.SetOrRemoveAnnotation(
            SnowflakeAnnotationNames.IdentitySeed,
            seed,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the default schema.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the default schema.</returns>
    public static ConfigurationSource? GetIdentitySeedConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(SnowflakeAnnotationNames.IdentitySeed)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the default identity increment.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The default identity increment.</returns>
    public static int GetIdentityIncrement(this IReadOnlyModel model)
        => (model is RuntimeModel)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (int?)model[SnowflakeAnnotationNames.IdentityIncrement] ?? 1;

    /// <summary>
    ///     Sets the default identity increment.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="increment">The value to set.</param>
    public static void SetIdentityIncrement(this IMutableModel model, int? increment)
        => model.SetOrRemoveAnnotation(
            SnowflakeAnnotationNames.IdentityIncrement,
            increment);

    /// <summary>
    ///     Sets the default identity increment.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="increment">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static int? SetIdentityIncrement(
        this IConventionModel model,
        int? increment,
        bool fromDataAnnotation = false)
        => (int?)model.SetOrRemoveAnnotation(
            SnowflakeAnnotationNames.IdentityIncrement,
            increment,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the default identity increment.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the default identity increment.</returns>
    public static ConfigurationSource? GetIdentityIncrementConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(SnowflakeAnnotationNames.IdentityIncrement)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the <see cref="SnowflakeValueGenerationStrategy" /> to use for properties
    ///     of keys in the model, unless the property has a strategy explicitly set.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The default <see cref="SnowflakeValueGenerationStrategy" />.</returns>
    public static SnowflakeValueGenerationStrategy? GetValueGenerationStrategy(this IReadOnlyModel model)
        => (SnowflakeValueGenerationStrategy?)model[SnowflakeAnnotationNames.ValueGenerationStrategy];

    /// <summary>
    ///     Sets the <see cref="SnowflakeValueGenerationStrategy" /> to use for properties
    ///     of keys in the model that don't have a strategy explicitly set.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="value">The value to set.</param>
    public static void SetValueGenerationStrategy(
        this IMutableModel model,
        SnowflakeValueGenerationStrategy? value)
        => model.SetOrRemoveAnnotation(SnowflakeAnnotationNames.ValueGenerationStrategy, value);

    /// <summary>
    ///     Sets the <see cref="SnowflakeValueGenerationStrategy" /> to use for properties
    ///     of keys in the model that don't have a strategy explicitly set.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static SnowflakeValueGenerationStrategy? SetValueGenerationStrategy(
        this IConventionModel model,
        SnowflakeValueGenerationStrategy? value,
        bool fromDataAnnotation = false)
        => (SnowflakeValueGenerationStrategy?)model.SetOrRemoveAnnotation(
            SnowflakeAnnotationNames.ValueGenerationStrategy,
            value,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the default <see cref="SnowflakeValueGenerationStrategy" />.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the default <see cref="SnowflakeValueGenerationStrategy" />.</returns>
    public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(SnowflakeAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();
}
