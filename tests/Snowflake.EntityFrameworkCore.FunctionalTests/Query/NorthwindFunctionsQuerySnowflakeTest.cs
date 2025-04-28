using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public class NorthwindFunctionsQuerySnowflakeTest : NorthwindFunctionsQueryRelationalTestBase<
    NorthwindQuerySnowflakeFixture<NoopModelCustomizer>>
{
    public NorthwindFunctionsQuerySnowflakeTest(
        NorthwindQuerySnowflakeFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Regex

    public override Task Regex_IsMatch_MethodCall(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(o => Regex.IsMatch(o.CustomerID, "^T.*$")));

    #endregion

    #region Invalid Operator (SNOW-1874078)

    [ConditionalTheory(Skip = "Skipped due SNOW-1874078")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Datetime_subtraction_TotalDays(bool async) => Task.CompletedTask;

    #endregion

    #region Unsupported Convert (EF #19606)

    [ConditionalTheory(Skip = "Skipped due https://github.com/dotnet/efcore/issues/19606")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Convert_ToBoolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due https://github.com/dotnet/efcore/issues/19606")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Convert_ToByte(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due https://github.com/dotnet/efcore/issues/19606")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Convert_ToDecimal(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due https://github.com/dotnet/efcore/issues/19606")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Convert_ToDouble(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due https://github.com/dotnet/efcore/issues/19606")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Convert_ToInt16(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due https://github.com/dotnet/efcore/issues/19606")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Convert_ToInt32(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due https://github.com/dotnet/efcore/issues/19606")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Convert_ToInt64(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due https://github.com/dotnet/efcore/issues/19606")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Convert_ToString(bool async) => Task.CompletedTask;

    #endregion
}