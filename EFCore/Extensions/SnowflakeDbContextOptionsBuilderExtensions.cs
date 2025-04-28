using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Snowflake.EntityFrameworkCore.Diagnostics;
using Snowflake.EntityFrameworkCore.Extensions;
using Snowflake.EntityFrameworkCore.Infrastructure;
using Snowflake.EntityFrameworkCore.Infrastructure.Internal;
using Snowflake.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Snowflake.EntityFrameworkCore;

/// <summary>
///     Snowflake specific extension methods for <see cref="DbContextOptionsBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake, Azure SQL, Azure Synapse databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SnowflakeDbContextOptionsExtensions
{
    /// <summary>
    ///     Configures the context to connect to a Snowflake database, but without initially setting any
    ///     <see cref="DbConnection" /> or connection string.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The connection or connection string must be set before the <see cref="DbContext" /> is used to connect
    ///         to a database. Set a connection using <see cref="RelationalDatabaseFacadeExtensions.SetDbConnection" />.
    ///         Set a connection string using <see cref="RelationalDatabaseFacadeExtensions.SetConnectionString" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///         <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake, Azure SQL, Azure Synapse databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="SnowflakeOptionsAction">An optional action to allow additional Snowflake specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseSnowflake(
        this DbContextOptionsBuilder optionsBuilder,
        Action<SnowflakeDbContextOptionsBuilder>? SnowflakeOptionsAction = null)
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(GetOrCreateExtension(optionsBuilder));

        return ApplyConfiguration(optionsBuilder, SnowflakeOptionsAction);
    }

    /// <summary>
    ///     Configures the context to connect to a Snowflake database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake, Azure SQL, Azure Synapse databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="SnowflakeOptionsAction">An optional action to allow additional Snowflake specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseSnowflake(
        this DbContextOptionsBuilder optionsBuilder,
        string? connectionString,
        Action<SnowflakeDbContextOptionsBuilder>? SnowflakeOptionsAction = null)
    {
        var extension = (SnowflakeOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return ApplyConfiguration(optionsBuilder, SnowflakeOptionsAction);
    }

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Microsoft Snowflake database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connection">
    ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
    ///     in the open state then EF will not open or close the connection. If the connection is in the closed
    ///     state then EF will open and close the connection as needed. The caller owns the connection and is
    ///     responsible for its disposal.
    /// </param>
    /// <param name="SnowflakeOptionsAction">An optional action to allow additional Snowflake specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseSnowflake(
        this DbContextOptionsBuilder optionsBuilder,
        DbConnection connection,
        Action<SnowflakeDbContextOptionsBuilder>? SnowflakeOptionsAction = null)
        => UseSnowflake(optionsBuilder, connection, false, SnowflakeOptionsAction);

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Microsoft Snowflake database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connection">
    ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
    ///     in the open state then EF will not open or close the connection. If the connection is in the closed
    ///     state then EF will open and close the connection as needed.
    /// </param>
    /// <param name="contextOwnsConnection">
    ///     If <see langword="true" />, then EF will take ownership of the connection and will
    ///     dispose it in the same way it would dispose a connection created by EF. If <see langword="false" />, then the caller still
    ///     owns the connection and is responsible for its disposal.
    /// </param>
    /// <param name="SnowflakeOptionsAction">An optional action to allow additional Snowflake specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseSnowflake(
        this DbContextOptionsBuilder optionsBuilder,
        DbConnection connection,
        bool contextOwnsConnection,
        Action<SnowflakeDbContextOptionsBuilder>? SnowflakeOptionsAction = null)
    {
        Check.NotNull(connection, nameof(connection));

        var extension = (SnowflakeOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnection(connection, contextOwnsConnection);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return ApplyConfiguration(optionsBuilder, SnowflakeOptionsAction);
    }

    /// <summary>
    ///     Configures the context to connect to a Microsoft Snowflake database, but without initially setting any
    ///     <see cref="DbConnection" /> or connection string.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The connection or connection string must be set before the <see cref="DbContext" /> is used to connect
    ///         to a database. Set a connection using <see cref="RelationalDatabaseFacadeExtensions.SetDbConnection" />.
    ///         Set a connection string using <see cref="RelationalDatabaseFacadeExtensions.SetConnectionString" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///         <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="SnowflakeOptionsAction">An optional action to allow additional Snowflake specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseSnowflake<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        Action<SnowflakeDbContextOptionsBuilder>? SnowflakeOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseSnowflake(
            (DbContextOptionsBuilder)optionsBuilder, SnowflakeOptionsAction);

    /// <summary>
    ///     Configures the context to connect to a Microsoft Snowflake database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="SnowflakeOptionsAction">An optional action to allow additional Snowflake specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseSnowflake<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string? connectionString,
        Action<SnowflakeDbContextOptionsBuilder>? SnowflakeOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseSnowflake(
            (DbContextOptionsBuilder)optionsBuilder, connectionString, SnowflakeOptionsAction);

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Microsoft Snowflake database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connection">
    ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
    ///     in the open state then EF will not open or close the connection. If the connection is in the closed
    ///     state then EF will open and close the connection as needed. The caller owns the connection and is
    ///     responsible for its disposal.
    /// </param>
    /// <param name="SnowflakeOptionsAction">An optional action to allow additional Snowflake specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseSnowflake<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DbConnection connection,
        Action<SnowflakeDbContextOptionsBuilder>? SnowflakeOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseSnowflake(
            (DbContextOptionsBuilder)optionsBuilder, connection, SnowflakeOptionsAction);
    
    public static DbContextOptionsBuilder UseEnvironment(
        this DbContextOptionsBuilder optionsBuilder,
        EnvironmentType environmentType)
    {
        Check.NotNull(optionsBuilder, nameof(optionsBuilder));
        Check.NotNull(environmentType, nameof(environmentType));

        var extension = GetOrCreateExtension(optionsBuilder).WithEnvironment(environmentType);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        ConfigureWarnings(optionsBuilder);

        return optionsBuilder;
    }
        
    public static DbContextOptionsBuilder<TContext> UseEnvironment<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder, 
        EnvironmentType environmentType)
        where TContext : DbContext
    {
        Check.NotNull(optionsBuilder, nameof(optionsBuilder));
        Check.NotNull(environmentType, nameof(environmentType));

        var extension = GetOrCreateExtension(optionsBuilder).WithEnvironment(environmentType);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        ConfigureWarnings(optionsBuilder);

        return optionsBuilder;
    }

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Microsoft Snowflake database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connection">
    ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
    ///     in the open state then EF will not open or close the connection. If the connection is in the closed
    ///     state then EF will open and close the connection as needed.
    /// </param>
    /// <param name="contextOwnsConnection">
    ///     If <see langword="true" />, then EF will take ownership of the connection and will
    ///     dispose it in the same way it would dispose a connection created by EF. If <see langword="false" />, then the caller still
    ///     owns the connection and is responsible for its disposal.
    /// </param>
    /// <param name="SnowflakeOptionsAction">An optional action to allow additional Snowflake specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseSnowflake<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DbConnection connection,
        bool contextOwnsConnection,
        Action<SnowflakeDbContextOptionsBuilder>? SnowflakeOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseSnowflake(
            (DbContextOptionsBuilder)optionsBuilder, connection, contextOwnsConnection, SnowflakeOptionsAction);

    private static SnowflakeOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.Options.FindExtension<SnowflakeOptionsExtension>()
            ?? new SnowflakeOptionsExtension();

    private static DbContextOptionsBuilder ApplyConfiguration(
        DbContextOptionsBuilder optionsBuilder,
        Action<SnowflakeDbContextOptionsBuilder>? SnowflakeOptionsAction)
    {
        ConfigureWarnings(optionsBuilder);

        SnowflakeOptionsAction?.Invoke(new SnowflakeDbContextOptionsBuilder(optionsBuilder));

        var extension = (SnowflakeOptionsExtension)GetOrCreateExtension(optionsBuilder);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsExtension
            = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        coreOptionsExtension = RelationalOptionsExtension.WithDefaultWarningConfiguration(coreOptionsExtension);

        coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
            coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                SnowflakeEventId.ConflictingValueGenerationStrategiesWarning, WarningBehavior.Throw));

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
    }
}
