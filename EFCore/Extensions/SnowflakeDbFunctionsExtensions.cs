using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Snowflake.EntityFrameworkCore;

/// <summary>
///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
///     The methods on this class are accessed via <see cref="EF.Functions" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SnowflakeDbFunctionsExtensions
{
    #region Full-text search

    /// <summary>
    ///     A DbFunction method stub that can be used in LINQ queries to target the Snowflake <c>FREETEXT</c> store function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="propertyReference">The property on which the search will be performed.</param>
    /// <param name="freeText">The text that will be searched for in the property.</param>
    /// <param name="languageTerm">A Language ID from the <c>sys.syslanguages</c> table.</param>
    public static bool FreeText(
        this DbFunctions _,
        object propertyReference,
        string freeText,
        [NotParameterized] int languageTerm)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FreeText)));

    /// <summary>
    ///     A DbFunction method stub that can be used in LINQ queries to target the Snowflake <c>FREETEXT</c> store function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="propertyReference">The property on which the search will be performed.</param>
    /// <param name="freeText">The text that will be searched for in the property.</param>
    public static bool FreeText(
        this DbFunctions _,
        object propertyReference,
        string freeText)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FreeText)));

    /// <summary>
    ///     A DbFunction method stub that can be used in LINQ queries to target the Snowflake <c>CONTAINS</c> store function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="propertyReference">The property on which the search will be performed.</param>
    /// <param name="searchCondition">The text that will be searched for in the property and the condition for a match.</param>
    /// <param name="languageTerm">A Language ID from the <c>sys.syslanguages</c> table.</param>
    public static bool Contains(
        this DbFunctions _,
        object propertyReference,
        string searchCondition,
        [NotParameterized] int languageTerm)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    ///     A DbFunction method stub that can be used in LINQ queries to target the Snowflake <c>CONTAINS</c> store function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="propertyReference">The property on which the search will be performed.</param>
    /// <param name="searchCondition">The text that will be searched for in the property and the condition for a match.</param>
    public static bool Contains(
        this DbFunctions _,
        object propertyReference,
        string searchCondition)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    #endregion Full-text search

    #region DateDiffYear

    /// <summary>
    ///     Counts the number of year boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(year, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of year boundaries crossed between the dates.</returns>
    public static int DateDiffYear(
        this DbFunctions _,
        DateTime startDate,
        DateTime endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

    /// <summary>
    ///     Counts the number of year boundaries crossed between <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(year, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of year boundaries crossed between the dates.</returns>
    public static int? DateDiffYear(
        this DbFunctions _,
        DateTime? startDate,
        DateTime? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

    /// <summary>
    ///     Counts the number of year boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(year, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of year boundaries crossed between the dates.</returns>
    public static int DateDiffYear(
        this DbFunctions _,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

    /// <summary>
    ///     Counts the number of year boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(year, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of year boundaries crossed between the dates.</returns>
    public static int? DateDiffYear(
        this DbFunctions _,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

    /// <summary>
    ///     Counts the number of year boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(year, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of year boundaries crossed between the dates.</returns>
    public static int DateDiffYear(
        this DbFunctions _,
        DateOnly startDate,
        DateOnly endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

    /// <summary>
    ///     Counts the number of year boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(year, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of year boundaries crossed between the dates.</returns>
    public static int? DateDiffYear(
        this DbFunctions _,
        DateOnly? startDate,
        DateOnly? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

    #endregion DateDiffYear

    #region DateDiffMonth

    /// <summary>
    ///     Counts the number of month boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(month, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of month boundaries crossed between the dates.</returns>
    public static int DateDiffMonth(
        this DbFunctions _,
        DateTime startDate,
        DateTime endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

    /// <summary>
    ///     Counts the number of month boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(month, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of month boundaries crossed between the dates.</returns>
    public static int? DateDiffMonth(
        this DbFunctions _,
        DateTime? startDate,
        DateTime? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

    /// <summary>
    ///     Counts the number of month boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(month, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of month boundaries crossed between the dates.</returns>
    public static int DateDiffMonth(
        this DbFunctions _,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

    /// <summary>
    ///     Counts the number of month boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(month, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of month boundaries crossed between the dates.</returns>
    public static int? DateDiffMonth(
        this DbFunctions _,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

    /// <summary>
    ///     Counts the number of month boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(month, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of month boundaries crossed between the dates.</returns>
    public static int DateDiffMonth(
        this DbFunctions _,
        DateOnly startDate,
        DateOnly endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

    /// <summary>
    ///     Counts the number of month boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(month, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of month boundaries crossed between the dates.</returns>
    public static int? DateDiffMonth(
        this DbFunctions _,
        DateOnly? startDate,
        DateOnly? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

    #endregion DateDiffMonth

    #region DateDiffDay

    /// <summary>
    ///     Counts the number of day boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(day, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of day boundaries crossed between the dates.</returns>
    public static int DateDiffDay(
        this DbFunctions _,
        DateTime startDate,
        DateTime endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

    /// <summary>
    ///     Counts the number of day boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(day, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of day boundaries crossed between the dates.</returns>
    public static int? DateDiffDay(
        this DbFunctions _,
        DateTime? startDate,
        DateTime? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

    /// <summary>
    ///     Counts the number of day boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(day, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of day boundaries crossed between the dates.</returns>
    public static int DateDiffDay(
        this DbFunctions _,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

    /// <summary>
    ///     Counts the number of day boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(day, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of day boundaries crossed between the dates.</returns>
    public static int? DateDiffDay(
        this DbFunctions _,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

    /// <summary>
    ///     Counts the number of day boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(day, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of day boundaries crossed between the dates.</returns>
    public static int DateDiffDay(
        this DbFunctions _,
        DateOnly startDate,
        DateOnly endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

    /// <summary>
    ///     Counts the number of day boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(day, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of day boundaries crossed between the dates.</returns>
    public static int? DateDiffDay(
        this DbFunctions _,
        DateOnly? startDate,
        DateOnly? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

    #endregion DateDiffDay

    #region DateDiffHour

    /// <summary>
    ///     Counts the number of hour boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of hour boundaries crossed between the dates.</returns>
    public static int DateDiffHour(
        this DbFunctions _,
        DateTime startDate,
        DateTime endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

    /// <summary>
    ///     Counts the number of hour boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of hour boundaries crossed between the dates.</returns>
    public static int? DateDiffHour(
        this DbFunctions _,
        DateTime? startDate,
        DateTime? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

    /// <summary>
    ///     Counts the number of hour boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of hour boundaries crossed between the dates.</returns>
    public static int DateDiffHour(
        this DbFunctions _,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

    /// <summary>
    ///     Counts the number of hour boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of hour boundaries crossed between the dates.</returns>
    public static int? DateDiffHour(
        this DbFunctions _,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

    /// <summary>
    ///     Counts the number of hour boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(hour, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of hour boundaries crossed between the timespans.</returns>
    public static int DateDiffHour(
        this DbFunctions _,
        TimeSpan startTimeSpan,
        TimeSpan endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

    /// <summary>
    ///     Counts the number of hour boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(hour, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of hour boundaries crossed between the timespans.</returns>
    public static int? DateDiffHour(
        this DbFunctions _,
        TimeSpan? startTimeSpan,
        TimeSpan? endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

    /// <summary>
    ///     Counts the number of hour boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(hour, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of hour boundaries crossed between the times.</returns>
    public static int DateDiffHour(
        this DbFunctions _,
        TimeOnly startTime,
        TimeOnly endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

    /// <summary>
    ///     Counts the number of hour boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(hour, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of hour boundaries crossed between the times.</returns>
    public static int? DateDiffHour(
        this DbFunctions _,
        TimeOnly? startTime,
        TimeOnly? endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

    /// <summary>
    ///     Counts the number of hour boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of hour boundaries crossed between the dates.</returns>
    public static int DateDiffHour(
        this DbFunctions _,
        DateOnly startDate,
        DateOnly endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

    /// <summary>
    ///     Counts the number of hour boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(hour, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of hour boundaries crossed between the dates.</returns>
    public static int? DateDiffHour(
        this DbFunctions _,
        DateOnly? startDate,
        DateOnly? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

    #endregion DateDiffHour

    #region DateDiffMinute

    /// <summary>
    ///     Counts the number of minute boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of minute boundaries crossed between the dates.</returns>
    public static int DateDiffMinute(
        this DbFunctions _,
        DateTime startDate,
        DateTime endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

    /// <summary>
    ///     Counts the number of minute boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of minute boundaries crossed between the dates.</returns>
    public static int? DateDiffMinute(
        this DbFunctions _,
        DateTime? startDate,
        DateTime? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

    /// <summary>
    ///     Counts the number of minute boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of minute boundaries crossed between the dates.</returns>
    public static int DateDiffMinute(
        this DbFunctions _,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

    /// <summary>
    ///     Counts the number of minute boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of minute boundaries crossed between the dates.</returns>
    public static int? DateDiffMinute(
        this DbFunctions _,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

    /// <summary>
    ///     Counts the number of minute boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(minute, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of minute boundaries crossed between the timespans.</returns>
    public static int DateDiffMinute(
        this DbFunctions _,
        TimeSpan startTimeSpan,
        TimeSpan endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

    /// <summary>
    ///     Counts the number of minute boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(minute, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of minute boundaries crossed between the timespans.</returns>
    public static int? DateDiffMinute(
        this DbFunctions _,
        TimeSpan? startTimeSpan,
        TimeSpan? endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

    /// <summary>
    ///     Counts the number of minute boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(minute, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of minute boundaries crossed between the times.</returns>
    public static int DateDiffMinute(
        this DbFunctions _,
        TimeOnly startTime,
        TimeOnly endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

    /// <summary>
    ///     Counts the number of minute boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(minute, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of minute boundaries crossed between the times.</returns>
    public static int? DateDiffMinute(
        this DbFunctions _,
        TimeOnly? startTime,
        TimeOnly? endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

    /// <summary>
    ///     Counts the number of minute boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of minute boundaries crossed between the dates.</returns>
    public static int DateDiffMinute(
        this DbFunctions _,
        DateOnly startDate,
        DateOnly endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

    /// <summary>
    ///     Counts the number of minute boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(minute, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of minute boundaries crossed between the dates.</returns>
    public static int? DateDiffMinute(
        this DbFunctions _,
        DateOnly? startDate,
        DateOnly? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

    #endregion DateDiffMinute

    #region DateDiffSecond

    /// <summary>
    ///     Counts the number of second boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(second, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of second boundaries crossed between the dates.</returns>
    public static int DateDiffSecond(
        this DbFunctions _,
        DateTime startDate,
        DateTime endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

    /// <summary>
    ///     Counts the number of second boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(second, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of second boundaries crossed between the dates.</returns>
    public static int? DateDiffSecond(
        this DbFunctions _,
        DateTime? startDate,
        DateTime? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

    /// <summary>
    ///     Counts the number of second boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(second, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of second boundaries crossed between the dates.</returns>
    public static int DateDiffSecond(
        this DbFunctions _,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

    /// <summary>
    ///     Counts the number of second boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(second, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of second boundaries crossed between the dates.</returns>
    public static int? DateDiffSecond(
        this DbFunctions _,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

    /// <summary>
    ///     Counts the number of second boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(second, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of second boundaries crossed between the timespans.</returns>
    public static int DateDiffSecond(
        this DbFunctions _,
        TimeSpan startTimeSpan,
        TimeSpan endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

    /// <summary>
    ///     Counts the number of second boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(second, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of second boundaries crossed between the timespans.</returns>
    public static int? DateDiffSecond(
        this DbFunctions _,
        TimeSpan? startTimeSpan,
        TimeSpan? endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

    /// <summary>
    ///     Counts the number of second boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(second, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of second boundaries crossed between the times.</returns>
    public static int DateDiffSecond(
        this DbFunctions _,
        TimeOnly startTime,
        TimeOnly endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

    /// <summary>
    ///     Counts the number of second boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(second, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of second boundaries crossed between the times.</returns>
    public static int? DateDiffSecond(
        this DbFunctions _,
        TimeOnly? startTime,
        TimeOnly? endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

    /// <summary>
    ///     Counts the number of second boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(second, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of second boundaries crossed between the dates.</returns>
    public static int DateDiffSecond(
        this DbFunctions _,
        DateOnly startDate,
        DateOnly endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

    /// <summary>
    ///     Counts the number of second boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(second, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of second boundaries crossed between the dates.</returns>
    public static int? DateDiffSecond(
        this DbFunctions _,
        DateOnly? startDate,
        DateOnly? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

    #endregion DateDiffSecond

    #region DateDiffMillisecond

    /// <summary>
    ///     Counts the number of millisecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
    public static int DateDiffMillisecond(
        this DbFunctions _,
        DateTime startDate,
        DateTime endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

    /// <summary>
    ///     Counts the number of millisecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
    public static int? DateDiffMillisecond(
        this DbFunctions _,
        DateTime? startDate,
        DateTime? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

    /// <summary>
    ///     Counts the number of millisecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
    public static int DateDiffMillisecond(
        this DbFunctions _,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

    /// <summary>
    ///     Counts the number of millisecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
    public static int? DateDiffMillisecond(
        this DbFunctions _,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

    /// <summary>
    ///     Counts the number of millisecond boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(millisecond, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of millisecond boundaries crossed between the timespans.</returns>
    public static int DateDiffMillisecond(
        this DbFunctions _,
        TimeSpan startTimeSpan,
        TimeSpan endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

    /// <summary>
    ///     Counts the number of millisecond boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(millisecond, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of millisecond boundaries crossed between the timespans.</returns>
    public static int? DateDiffMillisecond(
        this DbFunctions _,
        TimeSpan? startTimeSpan,
        TimeSpan? endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

    /// <summary>
    ///     Counts the number of millisecond boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(millisecond, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of millisecond boundaries crossed between the times.</returns>
    public static int DateDiffMillisecond(
        this DbFunctions _,
        TimeOnly startTime,
        TimeOnly endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

    /// <summary>
    ///     Counts the number of millisecond boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(millisecond, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of millisecond boundaries crossed between the times.</returns>
    public static int? DateDiffMillisecond(
        this DbFunctions _,
        TimeOnly? startTime,
        TimeOnly? endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

    /// <summary>
    ///     Counts the number of millisecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
    public static int DateDiffMillisecond(
        this DbFunctions _,
        DateOnly startDate,
        DateOnly endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

    /// <summary>
    ///     Counts the number of millisecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(millisecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
    public static int? DateDiffMillisecond(
        this DbFunctions _,
        DateOnly? startDate,
        DateOnly? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

    #endregion DateDiffMillisecond

    #region DateDiffMicrosecond

    /// <summary>
    ///     Counts the number of microsecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
    public static int DateDiffMicrosecond(
        this DbFunctions _,
        DateTime startDate,
        DateTime endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

    /// <summary>
    ///     Counts the number of microsecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
    public static int? DateDiffMicrosecond(
        this DbFunctions _,
        DateTime? startDate,
        DateTime? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

    /// <summary>
    ///     Counts the number of microsecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
    public static int DateDiffMicrosecond(
        this DbFunctions _,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

    /// <summary>
    ///     Counts the number of microsecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
    public static int? DateDiffMicrosecond(
        this DbFunctions _,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

    /// <summary>
    ///     Counts the number of microsecond boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(microsecond, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of microsecond boundaries crossed between the timespans.</returns>
    public static int DateDiffMicrosecond(
        this DbFunctions _,
        TimeSpan startTimeSpan,
        TimeSpan endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

    /// <summary>
    ///     Counts the number of microsecond boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(microsecond, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of microsecond boundaries crossed between the timespans.</returns>
    public static int? DateDiffMicrosecond(
        this DbFunctions _,
        TimeSpan? startTimeSpan,
        TimeSpan? endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

    /// <summary>
    ///     Counts the number of microsecond boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(microsecond, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of microsecond boundaries crossed between the times.</returns>
    public static int DateDiffMicrosecond(
        this DbFunctions _,
        TimeOnly startTime,
        TimeOnly endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

    /// <summary>
    ///     Counts the number of microsecond boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(microsecond, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of microsecond boundaries crossed between the times.</returns>
    public static int? DateDiffMicrosecond(
        this DbFunctions _,
        TimeOnly? startTime,
        TimeOnly? endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

    /// <summary>
    ///     Counts the number of microsecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
    public static int DateDiffMicrosecond(
        this DbFunctions _,
        DateOnly startDate,
        DateOnly endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

    /// <summary>
    ///     Counts the number of microsecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(microsecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
    public static int? DateDiffMicrosecond(
        this DbFunctions _,
        DateOnly? startDate,
        DateOnly? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

    #endregion DateDiffMicrosecond

    #region DateDiffNanosecond

    /// <summary>
    ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
    public static int DateDiffNanosecond(
        this DbFunctions _,
        DateTime startDate,
        DateTime endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

    /// <summary>
    ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
    public static int? DateDiffNanosecond(
        this DbFunctions _,
        DateTime? startDate,
        DateTime? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

    /// <summary>
    ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
    public static int DateDiffNanosecond(
        this DbFunctions _,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

    /// <summary>
    ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
    public static int? DateDiffNanosecond(
        this DbFunctions _,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

    /// <summary>
    ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(nanosecond, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
    public static int DateDiffNanosecond(
        this DbFunctions _,
        TimeSpan startTimeSpan,
        TimeSpan endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

    /// <summary>
    ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startTimeSpan" /> and
    ///     <paramref name="endTimeSpan" />. Corresponds to Snowflake's <c>DATEDIFF(nanosecond, @startTimeSpan, @endTimeSpan)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTimeSpan">Starting timespan for the calculation.</param>
    /// <param name="endTimeSpan">Ending timespan for the calculation.</param>
    /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
    public static int? DateDiffNanosecond(
        this DbFunctions _,
        TimeSpan? startTimeSpan,
        TimeSpan? endTimeSpan)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

    /// <summary>
    ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(nanosecond, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of nanosecond boundaries crossed between the times.</returns>
    public static int DateDiffNanosecond(
        this DbFunctions _,
        TimeOnly startTime,
        TimeOnly endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

    /// <summary>
    ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startTime" /> and
    ///     <paramref name="endTime" />. Corresponds to Snowflake's <c>DATEDIFF(nanosecond, @startTime, @endTime)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startTime">Starting time for the calculation.</param>
    /// <param name="endTime">Ending time for the calculation.</param>
    /// <returns>Number of nanosecond boundaries crossed between the times.</returns>
    public static int? DateDiffNanosecond(
        this DbFunctions _,
        TimeOnly? startTime,
        TimeOnly? endTime)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

    /// <summary>
    ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
    public static int DateDiffNanosecond(
        this DbFunctions _,
        DateOnly startDate,
        DateOnly endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

    /// <summary>
    ///     Counts the number of nanosecond boundaries crossed between the <paramref name="startDate" /> and
    ///     <paramref name="endDate" />. Corresponds to Snowflake's <c>DATEDIFF(nanosecond, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
    public static int? DateDiffNanosecond(
        this DbFunctions _,
        DateOnly? startDate,
        DateOnly? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

    #endregion DateDiffNanosecond

    #region DateDiffWeek

    /// <summary>
    ///     Counts the number of week boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(week, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of week boundaries crossed between the dates.</returns>
    public static int DateDiffWeek(
        this DbFunctions _,
        DateTime startDate,
        DateTime endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

    /// <summary>
    ///     Counts the number of week boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(week, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of week boundaries crossed between the dates.</returns>
    public static int? DateDiffWeek(
        this DbFunctions _,
        DateTime? startDate,
        DateTime? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

    /// <summary>
    ///     Counts the number of week boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(week, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of week boundaries crossed between the dates.</returns>
    public static int DateDiffWeek(
        this DbFunctions _,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

    /// <summary>
    ///     Counts the number of week boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(week, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of week boundaries crossed between the dates.</returns>
    public static int? DateDiffWeek(
        this DbFunctions _,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

    /// <summary>
    ///     Counts the number of week boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(week, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of week boundaries crossed between the dates.</returns>
    public static int DateDiffWeek(
        this DbFunctions _,
        DateOnly startDate,
        DateOnly endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

    /// <summary>
    ///     Counts the number of week boundaries crossed between the <paramref name="startDate" /> and <paramref name="endDate" />.
    ///     Corresponds to Snowflake's <c>DATEDIFF(week, @startDate, @endDate)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="startDate">Starting date for the calculation.</param>
    /// <param name="endDate">Ending date for the calculation.</param>
    /// <returns>Number of week boundaries crossed between the dates.</returns>
    public static int? DateDiffWeek(
        this DbFunctions _,
        DateOnly? startDate,
        DateOnly? endDate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

    #endregion DateDiffWeek

    /// <summary>
    ///     Validate if the given string is a valid date.
    ///     Corresponds to Snowflake's <c>ISDATE('date')</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="expression">Expression to validate</param>
    /// <returns>true for valid date and false otherwise.</returns>
    public static bool IsDate(
        this DbFunctions _,
        string expression)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsDate)));

    /// <summary>
    ///     Initializes a new instance of the <see cref="DateTime" /> structure to the specified year, month, day.
    ///     Corresponds to Snowflake's <c>DATEFROMPARTS(year, month, day)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="year">The year (1753 through 9999).</param>
    /// <param name="month">The month (1 through 12).</param>
    /// <param name="day">The day (1 through the number of days in month).</param>
    /// <returns>New instance of the <see cref="DateTime" /> structure to the specified year, month, day.</returns>
    public static DateTime DateFromParts(
        this DbFunctions _,
        int year,
        int month,
        int day)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateFromParts)));

    /// <summary>
    ///     Returns the number of bytes used to represent any expression.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="arg">The value to be examined for data length.</param>
    /// <returns>The number of bytes in the input value.</returns>
    public static int? DataLength(
        this DbFunctions _,
        string? arg)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

    /// <summary>
    ///     Returns the number of bytes used to represent any expression.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="arg">The value to be examined for data length.</param>
    /// <returns>The number of bytes in the input value.</returns>
    public static int? DataLength(
        this DbFunctions _,
        bool? arg)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

    /// <summary>
    ///     Returns the number of bytes used to represent any expression.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="arg">The value to be examined for data length.</param>
    /// <returns>The number of bytes in the input value.</returns>
    public static int? DataLength(
        this DbFunctions _,
        double? arg)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

    /// <summary>
    ///     Returns the number of bytes used to represent any expression.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="arg">The value to be examined for data length.</param>
    /// <returns>The number of bytes in the input value.</returns>
    public static int? DataLength(
        this DbFunctions _,
        decimal? arg)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

    /// <summary>
    ///     Returns the number of bytes used to represent any expression.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="arg">The value to be examined for data length.</param>
    /// <returns>The number of bytes in the input value.</returns>
    public static int? DataLength(
        this DbFunctions _,
        DateTime? arg)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

    /// <summary>
    ///     Returns the number of bytes used to represent any expression.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="arg">The value to be examined for data length.</param>
    /// <returns>The number of bytes in the input value.</returns>
    public static int? DataLength(
        this DbFunctions _,
        TimeSpan? arg)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

    /// <summary>
    ///     Returns the number of bytes used to represent any expression.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="arg">The value to be examined for data length.</param>
    /// <returns>The number of bytes in the input value.</returns>
    public static int? DataLength(
        this DbFunctions _,
        DateTimeOffset? arg)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

    /// <summary>
    ///     Returns the number of bytes used to represent any expression.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="arg">The value to be examined for data length.</param>
    /// <returns>The number of bytes in the input value.</returns>
    public static int? DataLength(
        this DbFunctions _,
        byte[]? arg)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

    /// <summary>
    ///     Returns the number of bytes used to represent any expression.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="arg">The value to be examined for data length.</param>
    /// <returns>The number of bytes in the input value.</returns>
    public static int? DataLength(
        this DbFunctions _,
        Guid? arg)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DataLength)));

    /// <summary>
    ///     Validate if the given string is a valid numeric.
    ///     Corresponds to the Snowflake <c>ISNUMERIC(expression)</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="expression">Expression to validate</param>
    /// <returns><see langword="true" /> for a valid numeric, otherwise <see langword="false" />.</returns>
    public static bool IsNumeric(
        this DbFunctions _,
        string expression)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsNumeric)));

    /// <summary>
    ///     Converts <paramref name="dateTime" /> to the corresponding <c>datetimeoffset</c> in the target <paramref name="timeZone" />.
    ///     Corresponds to the Snowflake <c>AT TIME ZONE</c> construct.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that the <see cref="DateTime.Kind" /> of <paramref name="dateTime" /> is not taken into account when performing the
    ///         conversion; the offset for the provided time zone is simply applied as-is.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///         <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="dateTime">The value to convert to <c>datetimeoffset</c>.</param>
    /// <param name="timeZone">A valid Snowflake time zone ID.</param>
    /// <returns>The <c>datetimeoffset</c> resulting from the conversion.</returns>
    public static DateTimeOffset AtTimeZone(
        this DbFunctions _,
        DateTime dateTime,
        string timeZone)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(AtTimeZone)));

    /// <summary>
    ///     Converts <paramref name="dateTimeOffset" /> to the time zone specified by <paramref name="timeZone" />.
    ///     Corresponds to the Snowflake <c>AT TIME ZONE</c> construct.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="dateTimeOffset">The value on which to perform the time zone conversion.</param>
    /// <param name="timeZone">A valid Snowflake time zone ID.</param>
    /// <returns>The <c>datetimeoffset</c> resulting from the conversion.</returns>
    public static DateTimeOffset AtTimeZone(
        this DbFunctions _,
        DateTimeOffset dateTimeOffset,
        string timeZone)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(AtTimeZone)));

    /// <summary>
    ///     Returns the starting position of the first occurrence of a pattern in a specified expression, or zero if the pattern is not found, on all valid text and character data types.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="pattern">A string pattern to search for.</param>
    /// <param name="expression">Typically a column that is searched for the specified pattern</param>
    /// <returns>Returns the starting position of the first occurrence of a pattern in a specified expression.</returns>
    public static long PatIndex(
        this DbFunctions _,
        string pattern,
        string expression)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(PatIndex)));

    #region Sample standard deviation

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEV</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<byte> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEV</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<short> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEV</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<int> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEV</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<long> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEV</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<float> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEV</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<double> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    /// <summary>
    ///     Returns the sample standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEV</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample standard deviation.</returns>
    public static double? StandardDeviationSample(this DbFunctions _, IEnumerable<decimal> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationSample)));

    #endregion Sample standard deviation

    #region Population standard deviation

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEVP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<byte> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEVP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<short> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEVP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<int> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEVP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<long> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEVP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<float> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEVP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<double> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    /// <summary>
    ///     Returns the population standard deviation of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>STDEVP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population standard deviation.</returns>
    public static double? StandardDeviationPopulation(this DbFunctions _, IEnumerable<decimal> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StandardDeviationPopulation)));

    #endregion Population standard deviation

    #region Sample variance

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VAR</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<byte> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VAR</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<short> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VAR</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<int> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VAR</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<long> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VAR</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<float> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VAR</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<double> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    /// <summary>
    ///     Returns the sample variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VAR</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed sample variance.</returns>
    public static double? VarianceSample(this DbFunctions _, IEnumerable<decimal> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VarianceSample)));

    #endregion Sample variance

    #region Population variance

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VARP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<byte> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VARP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<short> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VARP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<int> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VARP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<long> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VARP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<float> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VARP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<double> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    /// <summary>
    ///     Returns the population variance of all values in the specified expression.
    ///     Corresponds to Snowflake's <c>VARP</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The computed population variance.</returns>
    public static double? VariancePopulation(this DbFunctions _, IEnumerable<decimal> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VariancePopulation)));

    #endregion Population variance
}
