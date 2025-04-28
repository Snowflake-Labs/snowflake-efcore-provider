using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snowflake.EntityFrameworkCore;
using Snowflake.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Metadata.Internal;
using Snowflake.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Snowflake specific extension methods for <see cref="ModelBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SnowflakeModelBuilderExtensions
{
    /// <summary>
    ///     Configures the model to use a sequence-based hi-lo pattern to generate values for key properties
    ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting Snowflake.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseHiLo(
        this ModelBuilder modelBuilder,
        string? name = null,
        string? schema = null)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        var model = modelBuilder.Model;

        name ??= SnowflakeModelExtensions.DefaultHiLoSequenceName;

        if (model.FindSequence(name, schema) == null)
        {
            modelBuilder.HasSequence(name, schema).IncrementsBy(10);
        }

        model.SetValueGenerationStrategy(SnowflakeValueGenerationStrategy.SequenceHiLo);
        model.SetHiLoSequenceName(name);
        model.SetHiLoSequenceSchema(schema);
        model.SetSequenceNameSuffix(null);
        model.SetSequenceSchema(null);
        model.SetIdentitySeed(null);
        model.SetIdentityIncrement(null);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the database sequence used for the hi-lo pattern to generate values for key properties
    ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting Snowflake.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the sequence.</returns>
    public static IConventionSequenceBuilder? HasHiLoSequence(
        this IConventionModelBuilder modelBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        if (!modelBuilder.CanSetHiLoSequence(name, schema))
        {
            return null;
        }

        modelBuilder.Metadata.SetHiLoSequenceName(name, fromDataAnnotation);
        modelBuilder.Metadata.SetHiLoSequenceSchema(schema, fromDataAnnotation);

        return name == null ? null : modelBuilder.HasSequence(name, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Returns a value indicating whether the given name and schema can be set for the hi-lo sequence.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given name and schema can be set for the hi-lo sequence.</returns>
    public static bool CanSetHiLoSequence(
        this IConventionModelBuilder modelBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        return modelBuilder.CanSetAnnotation(SnowflakeAnnotationNames.HiLoSequenceName, name, fromDataAnnotation)
            && modelBuilder.CanSetAnnotation(SnowflakeAnnotationNames.HiLoSequenceSchema, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Configures the model to use a sequence per hierarchy to generate values for key properties
    ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting Snowflake.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="nameSuffix">The name that will suffix the table name for each sequence created automatically.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseKeySequences(
        this ModelBuilder modelBuilder,
        string? nameSuffix = null,
        string? schema = null)
    {
        Check.NullButNotEmpty(nameSuffix, nameof(nameSuffix));
        Check.NullButNotEmpty(schema, nameof(schema));

        var model = modelBuilder.Model;

        nameSuffix ??= SnowflakeModelExtensions.DefaultSequenceNameSuffix;

        model.SetValueGenerationStrategy(SnowflakeValueGenerationStrategy.Sequence);
        model.SetSequenceNameSuffix(nameSuffix);
        model.SetSequenceSchema(schema);
        model.SetHiLoSequenceName(null);
        model.SetHiLoSequenceSchema(null);
        model.SetIdentitySeed(null);
        model.SetIdentityIncrement(null);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the model to use the Snowflake IDENTITY feature to generate values for key properties
    ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting Snowflake. This is the default
    ///     behavior when targeting Snowflake.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseIdentityColumns(
        this ModelBuilder modelBuilder,
        long seed = 1,
        int increment = 1)
    {
        var model = modelBuilder.Model;

        model.SetValueGenerationStrategy(SnowflakeValueGenerationStrategy.IdentityColumn);
        model.SetIdentitySeed(seed);
        model.SetIdentityIncrement(increment);
        model.SetSequenceNameSuffix(null);
        model.SetSequenceSchema(null);
        model.SetHiLoSequenceName(null);
        model.SetHiLoSequenceSchema(null);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the model to use the Snowflake IDENTITY feature to generate values for key properties
    ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting Snowflake. This is the default
    ///     behavior when targeting Snowflake.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseIdentityColumns(
        this ModelBuilder modelBuilder,
        int seed,
        int increment = 1)
        => modelBuilder.UseIdentityColumns((long)seed, increment);

    /// <summary>
    ///     Configures the default seed for Snowflake IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasIdentityColumnSeed(
        this IConventionModelBuilder modelBuilder,
        long? seed,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetIdentityColumnSeed(seed, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetIdentitySeed(seed, fromDataAnnotation);
            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the default seed for Snowflake IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the seed for Snowflake IDENTITY.</returns>
    public static bool CanSetIdentityColumnSeed(
        this IConventionModelBuilder modelBuilder,
        long? seed,
        bool fromDataAnnotation = false)
        => modelBuilder.CanSetAnnotation(SnowflakeAnnotationNames.IdentitySeed, seed, fromDataAnnotation);

    /// <summary>
    ///     Configures the default increment for Snowflake IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasIdentityColumnIncrement(
        this IConventionModelBuilder modelBuilder,
        int? increment,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetIdentityColumnIncrement(increment, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetIdentityIncrement(increment, fromDataAnnotation);
            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the default increment for Snowflake IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the default increment for Snowflake IDENTITY.</returns>
    public static bool CanSetIdentityColumnIncrement(
        this IConventionModelBuilder modelBuilder,
        int? increment,
        bool fromDataAnnotation = false)
        => modelBuilder.CanSetAnnotation(SnowflakeAnnotationNames.IdentityIncrement, increment, fromDataAnnotation);

    /// <summary>
    ///     Configures the default value generation strategy for key properties marked as <see cref="ValueGenerated.OnAdd" />,
    ///     when targeting Snowflake.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="valueGenerationStrategy">The value generation strategy.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasValueGenerationStrategy(
        this IConventionModelBuilder modelBuilder,
        SnowflakeValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
            if (valueGenerationStrategy != SnowflakeValueGenerationStrategy.IdentityColumn)
            {
                modelBuilder.HasIdentityColumnSeed(null, fromDataAnnotation);
                modelBuilder.HasIdentityColumnIncrement(null, fromDataAnnotation);
            }

            if (valueGenerationStrategy != SnowflakeValueGenerationStrategy.SequenceHiLo)
            {
                modelBuilder.HasHiLoSequence(null, null, fromDataAnnotation);
            }

            if (valueGenerationStrategy != SnowflakeValueGenerationStrategy.Sequence)
            {
                RemoveKeySequenceAnnotations();
            }

            return modelBuilder;
        }

        return null;

        void RemoveKeySequenceAnnotations()
        {
            if (modelBuilder.CanSetAnnotation(SnowflakeAnnotationNames.SequenceNameSuffix, null)
                && modelBuilder.CanSetAnnotation(SnowflakeAnnotationNames.SequenceSchema, null))
            {
                modelBuilder.Metadata.SetSequenceNameSuffix(null, fromDataAnnotation);
                modelBuilder.Metadata.SetSequenceSchema(null, fromDataAnnotation);
            }
        }
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the default value generation strategy.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="valueGenerationStrategy">The value generation strategy.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the default value generation strategy.</returns>
    public static bool CanSetValueGenerationStrategy(
        this IConventionModelBuilder modelBuilder,
        SnowflakeValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
        => modelBuilder.CanSetAnnotation(
            SnowflakeAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation);
}
