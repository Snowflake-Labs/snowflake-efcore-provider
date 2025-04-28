using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snowflake.EntityFrameworkCore.Metadata;

namespace Snowflake.EntityFrameworkCore;

/// <summary>
///     Snowflake specific extension methods for <see cref="ComplexTypePropertyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SnowflakeComplexTypePropertyBuilderExtensions
{
    /// <summary>
    ///     Configures the key property to use a sequence-based hi-lo pattern to generate values for new entities,
    ///     when targeting Snowflake. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder UseHiLo(
        this ComplexTypePropertyBuilder propertyBuilder,
        string? name = null,
        string? schema = null)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        var property = propertyBuilder.Metadata;

        name ??= SnowflakeModelExtensions.DefaultHiLoSequenceName;

        var model = property.DeclaringType.Model;

        if (model.FindSequence(name, schema) == null)
        {
            model.AddSequence(name, schema).IncrementBy = 10;
        }

        property.SetValueGenerationStrategy(SnowflakeValueGenerationStrategy.SequenceHiLo);
        property.SetHiLoSequenceName(name);
        property.SetHiLoSequenceSchema(schema);
        property.SetIdentitySeed(null);
        property.SetIdentityIncrement(null);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the key property to use a sequence-based hi-lo pattern to generate values for new entities,
    ///     when targeting Snowflake. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> UseHiLo<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        string? name = null,
        string? schema = null)
        => (ComplexTypePropertyBuilder<TProperty>)UseHiLo((ComplexTypePropertyBuilder)propertyBuilder, name, schema);

    /// <summary>
    ///     Configures the key property to use a sequence-based key value generation pattern to generate values for new entities,
    ///     when targeting Snowflake. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder UseSequence(
        this ComplexTypePropertyBuilder propertyBuilder,
        string? name = null,
        string? schema = null)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        var property = propertyBuilder.Metadata;

        property.SetValueGenerationStrategy(SnowflakeValueGenerationStrategy.Sequence);
        property.SetSequenceName(name);
        property.SetSequenceSchema(schema);
        property.SetHiLoSequenceName(null);
        property.SetHiLoSequenceSchema(null);
        property.SetIdentitySeed(null);
        property.SetIdentityIncrement(null);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the key property to use a sequence-based key value generation pattern to generate values for new entities,
    ///     when targeting Snowflake. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> UseSequence<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        string? name = null,
        string? schema = null)
        => (ComplexTypePropertyBuilder<TProperty>)UseSequence((ComplexTypePropertyBuilder)propertyBuilder, name, schema);

    /// <summary>
    ///     Configures the key property to use the Snowflake IDENTITY feature to generate values for new entities,
    ///     when targeting Snowflake. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder UseIdentityColumn(
        this ComplexTypePropertyBuilder propertyBuilder,
        long seed = 1,
        int increment = 1)
    {
        var property = propertyBuilder.Metadata;
        property.SetValueGenerationStrategy(SnowflakeValueGenerationStrategy.IdentityColumn);
        property.SetIdentitySeed(seed);
        property.SetIdentityIncrement(increment);
        property.SetHiLoSequenceName(null);
        property.SetHiLoSequenceSchema(null);
        property.SetSequenceName(null);
        property.SetSequenceSchema(null);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the key property to use the Snowflake IDENTITY feature to generate values for new entities,
    ///     when targeting Snowflake. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder UseIdentityColumn(
        this ComplexTypePropertyBuilder propertyBuilder,
        int seed,
        int increment = 1)
        => propertyBuilder.UseIdentityColumn((long)seed, increment);

    /// <summary>
    ///     Configures the key property to use the Snowflake IDENTITY feature to generate values for new entities,
    ///     when targeting Snowflake. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> UseIdentityColumn<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        long seed = 1,
        int increment = 1)
        => (ComplexTypePropertyBuilder<TProperty>)UseIdentityColumn((ComplexTypePropertyBuilder)propertyBuilder, seed, increment);

    /// <summary>
    ///     Configures the key property to use the Snowflake IDENTITY feature to generate values for new entities,
    ///     when targeting Snowflake. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> UseIdentityColumn<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        int seed,
        int increment = 1)
        => (ComplexTypePropertyBuilder<TProperty>)UseIdentityColumn((ComplexTypePropertyBuilder)propertyBuilder, (long)seed, increment);
}
