using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Snowflake.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
public class TemporalPeriodPropertyBuilder : IInfrastructure<PropertyBuilder>
{
    private readonly PropertyBuilder _propertyBuilder;

    /// <summary>
    ///    Constructs a new instance of <see cref="TemporalPeriodPropertyBuilder" />.
    /// </summary>
    [EntityFrameworkInternal]
    public TemporalPeriodPropertyBuilder(PropertyBuilder propertyBuilder)
    {
        _propertyBuilder = propertyBuilder;
    }

    /// <summary>
    ///     Configures the column name the period property maps to.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using Snowflake temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual TemporalPeriodPropertyBuilder HasColumnName(string name)
    {
        // when column name is set explicitly, use the regular (i.e. non-convention) builder
        // so that the column name doesn't get uniquified
        _propertyBuilder.HasColumnName(name);

        return this;
    }

    /// <summary>
    ///     Configures the precision of the period property.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using Snowflake temporal tables with EF Core</see>
    ///     for more information.
    /// </remarks>
    /// <param name="precision">The precision of the period property.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual TemporalPeriodPropertyBuilder HasPrecision(int precision)
    {
        _propertyBuilder.HasPrecision(precision);

        return this;
    }

    PropertyBuilder IInfrastructure<PropertyBuilder>.Instance
        => _propertyBuilder;

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
