using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

[Trait("Category", "UseHybridTables")]
public class NorthwindWhereQuerySnowflakeTest : NorthwindWhereQueryRelationalTestBase<
    NorthwindQuerySnowflakeFixture<NoopModelCustomizer>>
{
    public NorthwindWhereQuerySnowflakeTest(
        NorthwindQuerySnowflakeFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task ElementAt_over_custom_projection_compared_to_not_null(bool async)
    {
        await base.ElementAt_over_custom_projection_compared_to_not_null(async);
    }

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task ElementAtOrDefault_over_custom_projection_compared_to_null(bool async)
    {
        await base.ElementAtOrDefault_over_custom_projection_compared_to_null(async);
    }

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FirstOrDefault_over_scalar_projection_compared_to_not_null(bool async)
    {
        await base.FirstOrDefault_over_scalar_projection_compared_to_not_null(async);
    }

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FirstOrDefault_over_scalar_projection_compared_to_null(bool async)
    {
        await base.FirstOrDefault_over_scalar_projection_compared_to_null(async);
    }

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Where_contains_on_navigation(bool async)
    {
        await base.Where_contains_on_navigation(async);
    }

    [ConditionalTheory(Skip = "Snowflake does not support millisecond while using the `DATE_PART` function")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Where_datetime_millisecond_component(bool async)
    {
        await base.Where_datetime_millisecond_component(async);
    }

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Where_subquery_FirstOrDefault_compared_to_entity(bool async)
    {
        await base.Where_subquery_FirstOrDefault_compared_to_entity(async);
    }

    public override async Task Where_bitwise_xor(bool async)
    {
        // Review when updated to EFCore 9.0 (https://github.com/dotnet/efcore/issues/34736)
        await AssertTranslationFailed(() => base.Where_bitwise_xor(async));

        AssertSql();
    }

    public override async Task Where_compare_constructed_equal(bool async)
    {
        // Anonymous type to constant comparison. https://github.com/dotnet/efcore/issues/14672
        await AssertTranslationFailed(() => base.Where_compare_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_constructed_multi_value_equal(bool async)
    {
        // Anonymous type to constant comparison. https://github.com/dotnet/efcore/issues/14672
        await AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_constructed_multi_value_not_equal(bool async)
    {
        // Anonymous type to constant comparison. https://github.com/dotnet/efcore/issues/14672
        await AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_equal(bool async)
    {
        // Anonymous type to constant comparison. https://github.com/dotnet/efcore/issues/14672
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_multi_value_equal(bool async)
    {
        // Anonymous type to constant comparison. https://github.com/dotnet/efcore/issues/14672
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_multi_value_not_equal(bool async)
    {
        // Anonymous type to constant comparison. https://github.com/dotnet/efcore/issues/14672
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_equal(bool async)
    {
        // Anonymous type to constant comparison. https://github.com/dotnet/efcore/issues/14672
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_multi_value_equal(bool async)
    {
        // Anonymous type to constant comparison. https://github.com/dotnet/efcore/issues/14672
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool async)
    {
        // Anonymous type to constant comparison. https://github.com/dotnet/efcore/issues/14672
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Like_with_non_string_column_using_ToString(bool async)
    {
        await base.Like_with_non_string_column_using_ToString(async);

        AssertSql(
            """
            SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
            FROM "Orders" AS "o"
            WHERE CAST("o"."OrderID" AS varchar) LIKE '%20%'
            """);
    }

    public override async Task Time_of_day_datetime(bool async)
    {
        await base.Time_of_day_datetime(async);

        AssertSql(
            """
            SELECT "o"."OrderDate"
            FROM "Orders" AS "o"
            """);
    }

    public override async Task Where_datetime_date_component(bool async)
    {
        await base.Where_datetime_date_component(async);

        AssertSql(
            """
            __myDatetime_0='1998-05-04T00:00:00.0000000' (DbType = DateTime)

            SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
            FROM "Orders" AS "o"
            WHERE CAST("o"."OrderDate" AS DATE) = :__myDatetime_0
            """);
    }

    public override async Task Where_datetime_today(bool async)
    {
        await base.Where_datetime_today(async);

        AssertSql(
            """
            SELECT "e"."EmployeeID", "e"."City", "e"."Country", "e"."FirstName", "e"."ReportsTo", "e"."Title"
            FROM "Employees" AS "e"
            """);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Query_with_offset(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Select(e => e.EmployeeID).Skip(5));

        AssertSql(
            """
            __p_0='5'

            SELECT "e"."EmployeeID"
            FROM "Employees" AS "e"
            ORDER BY (SELECT 1)
            LIMIT NULL
            OFFSET :__p_0
            """);
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected sealed override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}