using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Snowflake.EntityFrameworkCore.Internal;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public abstract class ComplexTypeQueryRelationalSnowflakeBaseTest<TFixture> : ComplexTypeQueryRelationalTestBase<TFixture>
    where TFixture : ComplexTypeQueryRelationalFixtureBase, new()
{
    protected ComplexTypeQueryRelationalSnowflakeBaseTest(TFixture fixture)
        : base(fixture)
    {
    }
    
    /// <inheritdoc />
    public override async Task Project_struct_complex_type_via_optional_navigation(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_struct_complex_type_via_optional_navigation(async));
    
        Assert.Equal(RelationalStrings.CannotProjectNullableComplexType("ValuedCustomer.ShippingAddress#AddressStruct"), exception.Message);
    }
    
    /// <inheritdoc />
    public override async Task Project_complex_type_via_optional_navigation(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_complex_type_via_optional_navigation(async));
    
        Assert.Equal(RelationalStrings.CannotProjectNullableComplexType("Customer.ShippingAddress#Address"), exception.Message);
    }
    
    /// <inheritdoc />
    public override async Task Same_entity_with_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(bool async)
        => Assert.Equal(
            SnowflakeStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Same_entity_with_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(async))).Message);

    internal void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}