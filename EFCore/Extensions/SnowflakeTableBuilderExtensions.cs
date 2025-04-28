using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Snowflake.EntityFrameworkCore;
using Snowflake.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;


/// <summary>
///     Snowflake specific extension methods for <see cref="TableBuilder" />.
/// </summary>
public static class SnowflakeTableBuilderExtensions
{
    #region IsTemporal

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using Snowflake temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="temporal">A value indicating whether the table is temporal.</param>
    /// <returns>An object that can be used to configure the temporal table.</returns>
    public static TemporalTableBuilder IsTemporal(
        this TableBuilder tableBuilder,
        bool temporal = true)
    {
        tableBuilder.Metadata.SetIsTemporal(temporal);

        return new TemporalTableBuilder(tableBuilder.GetInfrastructure());
    }

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using Snowflake temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the temporal table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder IsTemporal(
        this TableBuilder tableBuilder,
        Action<TemporalTableBuilder> buildAction)
    {
        tableBuilder.Metadata.SetIsTemporal(true);

        buildAction(new TemporalTableBuilder(tableBuilder.GetInfrastructure()));

        return tableBuilder;
    }

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using Snowflake temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="temporal">A value indicating whether the table is temporal.</param>
    /// <returns>An object that can be used to configure the temporal table.</returns>
    public static TemporalTableBuilder<TEntity> IsTemporal<TEntity>(
        this TableBuilder<TEntity> tableBuilder,
        bool temporal = true)
        where TEntity : class
    {
        tableBuilder.Metadata.SetIsTemporal(temporal);

        return new TemporalTableBuilder<TEntity>(tableBuilder.GetInfrastructure<EntityTypeBuilder<TEntity>>());
    }

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using Snowflake temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the temporal table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder<TEntity> IsTemporal<TEntity>(
        this TableBuilder<TEntity> tableBuilder,
        Action<TemporalTableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        tableBuilder.Metadata.SetIsTemporal(true);
        buildAction(new TemporalTableBuilder<TEntity>(tableBuilder.GetInfrastructure<EntityTypeBuilder<TEntity>>()));

        return tableBuilder;
    }

    #endregion IsTemporal
    
    #region IsHybridTable

    /// <summary>
    ///     Configures the table as hybrid.
    /// </summary>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="hybrid">A value indicating whether the table is hybrid.</param>
    /// <returns>An object that can be used to configure the hybrid table.</returns>
    public static HybridTableBuilder IsHybridTable(
        this TableBuilder tableBuilder,
        bool hybrid = true)
    {
        tableBuilder.Metadata.SetIsHybridTable(hybrid);

        return new HybridTableBuilder(tableBuilder.GetInfrastructure());
    }

    /// <summary>
    ///     Configures the table as standard.
    /// </summary>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the temporal table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder IsHybridTable(
        this TableBuilder tableBuilder,
        Action<HybridTableBuilder> buildAction)
    {
        tableBuilder.Metadata.SetIsHybridTable(true);

        buildAction(new HybridTableBuilder(tableBuilder.GetInfrastructure()));

        return tableBuilder;
    }

    /// <summary>
    ///     Configures the table as standard.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="hybrid">A value indicating whether the table is standard.</param>
    /// <returns>An object that can be used to configure the temporal table.</returns>
    public static HybridTableBuilder<TEntity> IsHybridTable<TEntity>(
        this TableBuilder<TEntity> tableBuilder,
        bool hybrid = true)
        where TEntity : class
    {
        tableBuilder.Metadata.SetIsHybridTable(hybrid);

        return new HybridTableBuilder<TEntity>(tableBuilder.GetInfrastructure<EntityTypeBuilder<TEntity>>());
    }

    /// <summary>
    ///     Configures the table as standard.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the standard table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder<TEntity> IsHybridTable<TEntity>(
        this TableBuilder<TEntity> tableBuilder,
        Action<HybridTableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        tableBuilder.Metadata.SetIsHybridTable(true);
        buildAction(new HybridTableBuilder<TEntity>(tableBuilder.GetInfrastructure<EntityTypeBuilder<TEntity>>()));

        return tableBuilder;
    }
    
    #endregion IsHybridTable

}
