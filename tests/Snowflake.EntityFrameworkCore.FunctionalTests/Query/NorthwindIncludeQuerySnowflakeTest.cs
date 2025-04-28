using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public class NorthwindIncludeQuerySnowflakeTest : NorthwindIncludeQueryRelationalTestBase<
    NorthwindQuerySnowflakeFixture<NoopModelCustomizer>>
{
    public NorthwindIncludeQuerySnowflakeTest(NorthwindQuerySnowflakeFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region APPLY Invalid Operation Exception

    public override async Task Filtered_include_with_multiple_ordering(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Filtered_include_with_multiple_ordering(async));

    public override async Task Include_collection_with_cross_apply_with_filter(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Filtered_include_with_multiple_ordering(async));

    public override async Task Include_collection_with_outer_apply_with_filter(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Filtered_include_with_multiple_ordering(async));

    public override async Task Include_collection_with_outer_apply_with_filter_non_equality(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Filtered_include_with_multiple_ordering(async));

    #endregion
    
    #region Unsupported subquery
    
    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Include_collection_order_by_collection_column(bool async) => Task.CompletedTask;
    
    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Include_collection_order_by_subquery(bool async) => Task.CompletedTask;
    
    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Then_include_collection_order_by_collection_column(bool async) => Task.CompletedTask;
    
    #endregion
}