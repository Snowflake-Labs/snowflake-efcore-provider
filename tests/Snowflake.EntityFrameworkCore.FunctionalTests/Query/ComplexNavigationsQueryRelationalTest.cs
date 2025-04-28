using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

/// <summary>
/// The entire test class is skipped due to an issue with MULTISTATEMENT (INSERT followed by SELECT,
/// with '?' parameters in the INSERT values).
/// Bug: SNOW-1883691.
/// Once fixed, remove the Skip property from the ConditionalTheory attribute.
/// </summary>
public class ComplexNavigationsQuerySnowflakeTest : ComplexNavigationsQueryRelationalTestBase<ComplexNavigationsQuerySnowflakeFixture>
{
    public ComplexNavigationsQuerySnowflakeTest(ComplexNavigationsQuerySnowflakeFixture fixture)
        : base(fixture)
    {
    }
    
    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Entity_equality_empty(bool async)
    {
      return base.Entity_equality_empty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Key_equality_when_sentinel_ef_property(bool async)
    {
      return base.Key_equality_when_sentinel_ef_property(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Key_equality_using_property_method_required(bool async)
    {
      return base.Key_equality_using_property_method_required(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Key_equality_using_property_method_required2(bool async)
    {
      return base.Key_equality_using_property_method_required2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Key_equality_using_property_method_nested(bool async)
    {
      return base.Key_equality_using_property_method_nested(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Key_equality_using_property_method_nested2(bool async)
    {
      return base.Key_equality_using_property_method_nested2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Key_equality_using_property_method_and_member_expression1(bool async)
    {
      return base.Key_equality_using_property_method_and_member_expression1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Key_equality_using_property_method_and_member_expression2(bool async)
    {
      return base.Key_equality_using_property_method_and_member_expression2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Key_equality_using_property_method_and_member_expression3(bool async)
    {
      return base.Key_equality_using_property_method_and_member_expression3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Key_equality_navigation_converted_to_FK(bool async)
    {
      return base.Key_equality_navigation_converted_to_FK(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Key_equality_two_conditions_on_same_navigation(bool async)
    {
      return base.Key_equality_two_conditions_on_same_navigation(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Key_equality_two_conditions_on_same_navigation2(bool async)
    {
      return base.Key_equality_two_conditions_on_same_navigation2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Multi_level_include_with_short_circuiting(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_key_access_optional(bool async)
    {
      return base.Join_navigation_key_access_optional(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_key_access_required(bool async)
    {
      return base.Join_navigation_key_access_required(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Navigation_key_access_optional_comparison(bool async)
    {
      return base.Navigation_key_access_optional_comparison(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Simple_level1_include(bool async)
    {
      return base.Simple_level1_include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Simple_level1(bool async)
    {
      return base.Simple_level1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Simple_level1_level2_include(bool async)
    {
      return base.Simple_level1_level2_include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Simple_level1_level2_GroupBy_Count(bool async)
    {
      return base.Simple_level1_level2_GroupBy_Count(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Simple_level1_level2_GroupBy_Having_Count(bool async)
    {
      return base.Simple_level1_level2_GroupBy_Having_Count(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Simple_level1_level2_level3_include(bool async)
    {
      return base.Simple_level1_level2_level3_include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Navigation_key_access_required_comparison(bool async)
    {
      return base.Navigation_key_access_required_comparison(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Navigation_inside_method_call_translated_to_join(bool async)
    {
      return base.Navigation_inside_method_call_translated_to_join(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Navigation_inside_method_call_translated_to_join2(bool async)
    {
      return base.Navigation_inside_method_call_translated_to_join2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_inside_method_call_translated_to_join(bool async)
    {
      return base.Optional_navigation_inside_method_call_translated_to_join(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_inside_property_method_translated_to_join(bool async)
    {
      return base.Optional_navigation_inside_property_method_translated_to_join(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_inside_nested_method_call_translated_to_join(bool async)
    {
      return base.Optional_navigation_inside_nested_method_call_translated_to_join(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(bool async)
    {
      return base.Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability(bool async)
    {
      return base.Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability(bool async)
    {
      return base.Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments(bool async)
    {
      return base.Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_in_outer_selector_translated_to_extra_join(bool async)
    {
      return base.Join_navigation_in_outer_selector_translated_to_extra_join(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_in_outer_selector_translated_to_extra_join_nested(bool async)
    {
      return base.Join_navigation_in_outer_selector_translated_to_extra_join_nested(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_in_outer_selector_translated_to_extra_join_nested2(bool async)
    {
      return base.Join_navigation_in_outer_selector_translated_to_extra_join_nested2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_in_inner_selector(bool async)
    {
      return base.Join_navigation_in_inner_selector(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigations_in_inner_selector_translated_without_collision(bool async)
    {
      return base.Join_navigations_in_inner_selector_translated_without_collision(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_non_key_join(bool async)
    {
      return base.Join_navigation_non_key_join(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_with_orderby_on_inner_sequence_navigation_non_key_join(bool async)
    {
      return base.Join_with_orderby_on_inner_sequence_navigation_non_key_join(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_self_ref(bool async)
    {
      return base.Join_navigation_self_ref(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_nested(bool async)
    {
      return base.Join_navigation_nested(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_nested2(bool async)
    {
      return base.Join_navigation_nested2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_deeply_nested_non_key_join(bool async)
    {
      return base.Join_navigation_deeply_nested_non_key_join(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_navigation_deeply_nested_required(bool async)
    {
      return base.Join_navigation_deeply_nested_required(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include_reference_and_project_into_anonymous_type(bool async)
    {
      return base.Include_reference_and_project_into_anonymous_type(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_nav_prop_reference_optional1(bool async)
    {
      return base.Select_nav_prop_reference_optional1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_nav_prop_reference_optional1_via_DefaultIfEmpty(bool async)
    {
      return base.Select_nav_prop_reference_optional1_via_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_nav_prop_reference_optional2(bool async)
    {
      return base.Select_nav_prop_reference_optional2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_nav_prop_reference_optional2_via_DefaultIfEmpty(bool async)
    {
      return base.Select_nav_prop_reference_optional2_via_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_nav_prop_reference_optional3(bool async)
    {
      return base.Select_nav_prop_reference_optional3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_nav_prop_reference_optional1(bool async)
    {
      return base.Where_nav_prop_reference_optional1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_nav_prop_reference_optional1_via_DefaultIfEmpty(bool async)
    {
      return base.Where_nav_prop_reference_optional1_via_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_nav_prop_reference_optional2(bool async)
    {
      return base.Where_nav_prop_reference_optional2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_nav_prop_reference_optional2_via_DefaultIfEmpty(bool async)
    {
      return base.Where_nav_prop_reference_optional2_via_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_multiple_nav_prop_reference_optional(bool async)
    {
      return base.Select_multiple_nav_prop_reference_optional(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_multiple_nav_prop_reference_optional_member_compared_to_value(bool async)
    {
      return base.Where_multiple_nav_prop_reference_optional_member_compared_to_value(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_multiple_nav_prop_reference_optional_member_compared_to_null(bool async)
    {
      return base.Where_multiple_nav_prop_reference_optional_member_compared_to_null(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_multiple_nav_prop_reference_optional_compared_to_null1(bool async)
    {
      return base.Where_multiple_nav_prop_reference_optional_compared_to_null1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_multiple_nav_prop_reference_optional_compared_to_null2(bool async)
    {
      return base.Where_multiple_nav_prop_reference_optional_compared_to_null2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_multiple_nav_prop_reference_optional_compared_to_null3(bool async)
    {
      return base.Where_multiple_nav_prop_reference_optional_compared_to_null3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_multiple_nav_prop_reference_optional_compared_to_null4(bool async)
    {
      return base.Where_multiple_nav_prop_reference_optional_compared_to_null4(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_multiple_nav_prop_reference_optional_compared_to_null5(bool async)
    {
      return base.Where_multiple_nav_prop_reference_optional_compared_to_null5(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_multiple_nav_prop_reference_required(bool async)
    {
      return base.Select_multiple_nav_prop_reference_required(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_multiple_nav_prop_reference_required2(bool async)
    {
      return base.Select_multiple_nav_prop_reference_required2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_multiple_nav_prop_optional_required(bool async)
    {
      return base.Select_multiple_nav_prop_optional_required(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_multiple_nav_prop_optional_required(bool async)
    {
      return base.Where_multiple_nav_prop_optional_required(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_navigation_comparison1(bool async)
    {
      return base.SelectMany_navigation_comparison1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_navigation_comparison2(bool async)
    {
      return base.SelectMany_navigation_comparison2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_navigation_comparison3(bool async)
    {
      return base.SelectMany_navigation_comparison3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_complex_predicate_with_with_nav_prop_and_OrElse1(bool async)
    {
      return base.Where_complex_predicate_with_with_nav_prop_and_OrElse1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_complex_predicate_with_with_nav_prop_and_OrElse2(bool async)
    {
      return base.Where_complex_predicate_with_with_nav_prop_and_OrElse2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_complex_predicate_with_with_nav_prop_and_OrElse3(bool async)
    {
      return base.Where_complex_predicate_with_with_nav_prop_and_OrElse3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_complex_predicate_with_with_nav_prop_and_OrElse4(bool async)
    {
      return base.Where_complex_predicate_with_with_nav_prop_and_OrElse4(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Complex_navigations_with_predicate_projected_into_anonymous_type(bool async)
    {
      return base.Complex_navigations_with_predicate_projected_into_anonymous_type(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Complex_navigations_with_predicate_projected_into_anonymous_type2(bool async)
    {
      return base.Complex_navigations_with_predicate_projected_into_anonymous_type2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_projected_into_DTO(bool async)
    {
      return base.Optional_navigation_projected_into_DTO(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task OrderBy_nav_prop_reference_optional(bool async)
    {
      return base.OrderBy_nav_prop_reference_optional(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty(bool async)
    {
      return base.OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Result_operator_nav_prop_reference_optional_Sum(bool async)
    {
      return base.Result_operator_nav_prop_reference_optional_Sum(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Result_operator_nav_prop_reference_optional_Min(bool async)
    {
      return base.Result_operator_nav_prop_reference_optional_Min(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Result_operator_nav_prop_reference_optional_Max(bool async)
    {
      return base.Result_operator_nav_prop_reference_optional_Max(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Result_operator_nav_prop_reference_optional_Average(bool async)
    {
      return base.Result_operator_nav_prop_reference_optional_Average(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Result_operator_nav_prop_reference_optional_Average_with_identity_selector(bool async)
    {
      return base.Result_operator_nav_prop_reference_optional_Average_with_identity_selector(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Result_operator_nav_prop_reference_optional_Average_without_selector(bool async)
    {
      return base.Result_operator_nav_prop_reference_optional_Average_without_selector(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(bool async)
    {
      return base.Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include_with_optional_navigation(bool async)
    {
      return base.Include_with_optional_navigation(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_flattening_bug_4539(bool async)
    {
      return base.Join_flattening_bug_4539(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Query_source_materialization_bug_4547(bool async)
    {
      return base.Query_source_materialization_bug_4547(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_navigation_property(bool async)
    {
      return base.SelectMany_navigation_property(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_navigation_property_and_projection(bool async)
    {
      return base.SelectMany_navigation_property_and_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_navigation_property_and_filter_before(bool async)
    {
      return base.SelectMany_navigation_property_and_filter_before(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_navigation_property_and_filter_after(bool async)
    {
      return base.SelectMany_navigation_property_and_filter_after(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_nested_navigation_property_required(bool async)
    {
      return base.SelectMany_nested_navigation_property_required(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_nested_navigation_property_optional_and_projection(bool async)
    {
      return base.SelectMany_nested_navigation_property_optional_and_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_SelectMany_calls(bool async)
    {
      return base.Multiple_SelectMany_calls(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_navigation_property_with_another_navigation_in_subquery(bool async)
    {
      return base.SelectMany_navigation_property_with_another_navigation_in_subquery(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_navigation_property_to_collection(bool async)
    {
      return base.Where_navigation_property_to_collection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_navigation_property_to_collection2(bool async)
    {
      return base.Where_navigation_property_to_collection2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_navigation_property_to_collection_of_original_entity_type(bool async)
    {
      return base.Where_navigation_property_to_collection_of_original_entity_type(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
    {
      return base.Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join(bool async)
    {
      return base.Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
    {
      return base.Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
    {
      return base.Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_where_with_subquery(bool async)
    {
      return base.SelectMany_where_with_subquery(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1(bool async)
    {
      return base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2(bool async)
    {
      return base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3(bool async)
    {
      return base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access(bool async)
    {
      return base.Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(bool async)
    {
      return base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(bool async)
    {
      return base.Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_take_optional_navigation(bool async)
    {
      return base.Optional_navigation_take_optional_navigation(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Projection_select_correct_table_from_subquery_when_materialization_is_not_required(bool async)
    {
      return base.Projection_select_correct_table_from_subquery_when_materialization_is_not_required(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Projection_select_correct_table_with_anonymous_projection_in_subquery(bool async)
    {
      return base.Projection_select_correct_table_with_anonymous_projection_in_subquery(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins(bool async)
    {
      return base.Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_predicate_on_optional_reference_navigation(bool async)
    {
      return base.Where_predicate_on_optional_reference_navigation(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_string_based_Include1(bool async)
    {
      return base.SelectMany_with_string_based_Include1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_EF_Property_Include1(bool async)
    {
      return base.SelectMany_with_EF_Property_Include1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_string_based_Include2(bool async)
    {
      return base.SelectMany_with_string_based_Include2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_SelectMany_with_string_based_Include(bool async)
    {
      return base.Multiple_SelectMany_with_string_based_Include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_SelectMany_with_EF_Property_Include(bool async)
    {
      return base.Multiple_SelectMany_with_EF_Property_Include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_required_navigations_with_Include(bool async)
    {
      return base.Multiple_required_navigations_with_Include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_required_navigation_using_multiple_selects_with_Include(bool async)
    {
      return base.Multiple_required_navigation_using_multiple_selects_with_Include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_required_navigation_with_string_based_Include(bool async)
    {
      return base.Multiple_required_navigation_with_string_based_Include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_required_navigation_with_EF_Property_Include(bool async)
    {
      return base.Multiple_required_navigation_with_EF_Property_Include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_required_navigation_using_multiple_selects_with_string_based_Include(bool async)
    {
      return base.Multiple_required_navigation_using_multiple_selects_with_string_based_Include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_required_navigation_using_multiple_selects_with_EF_Property_Include(bool async)
    {
      return base.Multiple_required_navigation_using_multiple_selects_with_EF_Property_Include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_with_Include(bool async)
    {
      return base.Optional_navigation_with_Include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool async)
    {
      return base.SelectMany_with_navigation_and_explicit_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
    {
      return base.SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(bool async)
    {
      return base.SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
    {
      return base.SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
    {
      return base.SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany(bool async)
    {
      return base.SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(bool async)
    {
      return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(bool async)
    {
      return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(bool async)
    {
      return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(bool async)
    {
      return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(bool async)
    {
      return base.Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(bool async)
    {
      return base.SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(bool async)
    {
      return base.SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool async)
    {
      return base.Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(bool async)
    {
      return base.SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_join_subquery_containing_filter_and_distinct(bool async)
    {
      return base.Select_join_subquery_containing_filter_and_distinct(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_join_with_key_selector_being_a_subquery(bool async)
    {
      return base.Select_join_with_key_selector_being_a_subquery(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Contains_with_subquery_optional_navigation_and_constant_item(bool async)
    {
      return base.Contains_with_subquery_optional_navigation_and_constant_item(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Contains_with_subquery_optional_navigation_scalar_distinct_and_constant_item(bool async)
    {
      return base.Contains_with_subquery_optional_navigation_scalar_distinct_and_constant_item(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Complex_query_with_optional_navigations_and_client_side_evaluation(bool async)
    {
      return base.Complex_query_with_optional_navigations_and_client_side_evaluation(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Required_navigation_on_a_subquery_with_First_in_projection(bool async)
    {
      return base.Required_navigation_on_a_subquery_with_First_in_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Required_navigation_on_a_subquery_with_complex_projection_and_First(bool async)
    {
      return base.Required_navigation_on_a_subquery_with_complex_projection_and_First(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Required_navigation_on_a_subquery_with_First_in_predicate(bool async)
    {
      return base.Required_navigation_on_a_subquery_with_First_in_predicate(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Manually_created_left_join_propagates_nullability_to_navigations(bool async)
    {
      return base.Manually_created_left_join_propagates_nullability_to_navigations(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_propagates_nullability_to_manually_created_left_join1(bool async)
    {
      return base.Optional_navigation_propagates_nullability_to_manually_created_left_join1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_propagates_nullability_to_manually_created_left_join2(bool async)
    {
      return base.Optional_navigation_propagates_nullability_to_manually_created_left_join2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Null_reference_protection_complex(bool async)
    {
      return base.Null_reference_protection_complex(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Null_reference_protection_complex_materialization(bool async)
    {
      return base.Null_reference_protection_complex_materialization(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Null_reference_protection_complex_client_eval(bool async)
    {
      return base.Null_reference_protection_complex_client_eval(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened(bool async)
    {
      return base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2(bool async)
    {
      return base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3(bool async)
    {
      return base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer(bool async)
    {
      return base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method(bool async)
    {
      return base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner(bool async)
    {
      return base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_on_a_subquery_containing_another_GroupJoin_with_orderby_on_inner_sequence_projecting_inner(bool async)
    {
      return base.GroupJoin_on_a_subquery_containing_another_GroupJoin_with_orderby_on_inner_sequence_projecting_inner(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_on_left_side_being_a_subquery(bool async)
    {
      return base.GroupJoin_on_left_side_being_a_subquery(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_on_right_side_being_a_subquery(bool async)
    {
      return base.GroupJoin_on_right_side_being_a_subquery(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_in_subquery_with_client_result_operator(bool async)
    {
      return base.GroupJoin_in_subquery_with_client_result_operator(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_in_subquery_with_client_projection(bool async)
    {
      return base.GroupJoin_in_subquery_with_client_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_in_subquery_with_client_projection_nested1(bool async)
    {
      return base.GroupJoin_in_subquery_with_client_projection_nested1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_in_subquery_with_client_projection_nested2(bool async)
    {
      return base.GroupJoin_in_subquery_with_client_projection_nested2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_client_method_on_outer(bool async)
    {
      return base.GroupJoin_client_method_on_outer(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_client_method_in_OrderBy(bool async)
    {
      return base.GroupJoin_client_method_in_OrderBy(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_without_DefaultIfEmpty(bool async)
    {
      return base.GroupJoin_without_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_with_subquery_on_inner(bool async)
    {
      return base.GroupJoin_with_subquery_on_inner(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty(bool async)
    {
      return base.GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Optional_navigation_in_subquery_with_unrelated_projection(bool async)
    {
      return base.Optional_navigation_in_subquery_with_unrelated_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Explicit_GroupJoin_in_subquery_with_unrelated_projection(bool async)
    {
      return base.Explicit_GroupJoin_in_subquery_with_unrelated_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Explicit_GroupJoin_in_subquery_with_unrelated_projection2(bool async)
    {
      return base.Explicit_GroupJoin_in_subquery_with_unrelated_projection2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Explicit_GroupJoin_in_subquery_with_unrelated_projection3(bool async)
    {
      return base.Explicit_GroupJoin_in_subquery_with_unrelated_projection3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Explicit_GroupJoin_in_subquery_with_unrelated_projection4(bool async)
    {
      return base.Explicit_GroupJoin_in_subquery_with_unrelated_projection4(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Explicit_GroupJoin_in_subquery_with_scalar_result_operator(bool async)
    {
      return base.Explicit_GroupJoin_in_subquery_with_scalar_result_operator(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Explicit_GroupJoin_in_subquery_with_multiple_result_operator_distinct_count_materializes_main_clause(bool async)
    {
      return base.Explicit_GroupJoin_in_subquery_with_multiple_result_operator_distinct_count_materializes_main_clause(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Where_on_multilevel_reference_in_subquery_with_outer_projection(bool async)
    {
      return base.Where_on_multilevel_reference_in_subquery_with_outer_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property(bool async)
    {
      return base.Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties(bool async)
    {
      return base.Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Nested_group_join_with_take(bool async)
    {
      return base.Nested_group_join_with_take(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Navigation_with_same_navigation_compared_to_null(bool async)
    {
      return base.Navigation_with_same_navigation_compared_to_null(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multi_level_navigation_compared_to_null(bool async)
    {
      return base.Multi_level_navigation_compared_to_null(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multi_level_navigation_with_same_navigation_compared_to_null(bool async)
    {
      return base.Multi_level_navigation_with_same_navigation_compared_to_null(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Navigations_compared_to_each_other1(bool async)
    {
      return base.Navigations_compared_to_each_other1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Navigations_compared_to_each_other2(bool async)
    {
      return base.Navigations_compared_to_each_other2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Navigations_compared_to_each_other3(bool async)
    {
      return base.Navigations_compared_to_each_other3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Navigations_compared_to_each_other4(bool async)
    {
      return base.Navigations_compared_to_each_other4(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Navigations_compared_to_each_other5(bool async)
    {
      return base.Navigations_compared_to_each_other5(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Level4_Include(bool async)
    {
      return base.Level4_Include(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Comparing_collection_navigation_on_optional_reference_to_null(bool async)
    {
      return base.Comparing_collection_navigation_on_optional_reference_to_null(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_subquery_with_client_eval_and_navigation1(bool async)
    {
      return base.Select_subquery_with_client_eval_and_navigation1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_subquery_with_client_eval_and_navigation2(bool async)
    {
      return base.Select_subquery_with_client_eval_and_navigation2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_subquery_with_client_eval_and_multi_level_navigation(bool async)
    {
      return base.Select_subquery_with_client_eval_and_multi_level_navigation(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Member_doesnt_get_pushed_down_into_subquery_with_result_operator(bool async)
    {
      return base.Member_doesnt_get_pushed_down_into_subquery_with_result_operator(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(bool async)
    {
      return base.Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Project_collection_navigation_count(bool async)
    {
      return base.Project_collection_navigation_count(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_optional_navigation_property_string_concat(bool async)
    {
      return base.Select_optional_navigation_property_string_concat(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Entries_for_detached_entities_are_removed(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include_reference_with_groupby_in_subquery(bool async)
    {
      return base.Include_reference_with_groupby_in_subquery(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multi_include_with_groupby_in_subquery(bool async)
    {
      return base.Multi_include_with_groupby_in_subquery(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task String_include_multiple_derived_navigation_with_same_name_and_same_type(bool async)
    {
      return base.String_include_multiple_derived_navigation_with_same_name_and_same_type(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task String_include_multiple_derived_navigation_with_same_name_and_different_type(bool async)
    {
      return base.String_include_multiple_derived_navigation_with_same_name_and_different_type(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task String_include_multiple_derived_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(bool async)
    {
      return base.String_include_multiple_derived_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task String_include_multiple_derived_collection_navigation_with_same_name_and_same_type(bool async)
    {
      return base.String_include_multiple_derived_collection_navigation_with_same_name_and_same_type(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task String_include_multiple_derived_collection_navigation_with_same_name_and_different_type(bool async)
    {
      return base.String_include_multiple_derived_collection_navigation_with_same_name_and_different_type(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task String_include_multiple_derived_collection_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(bool async)
    {
      return base.String_include_multiple_derived_collection_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task String_include_multiple_derived_navigations_complex(bool async)
    {
      return base.String_include_multiple_derived_navigations_complex(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Nav_rewrite_doesnt_apply_null_protection_for_function_arguments(bool async)
    {
      return base.Nav_rewrite_doesnt_apply_null_protection_for_function_arguments(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Accessing_optional_property_inside_result_operator_subquery(bool async)
    {
      return base.Accessing_optional_property_inside_result_operator_subquery(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_subquery_with_custom_projection(bool async)
    {
      return base.SelectMany_subquery_with_custom_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include1(bool async)
    {
      return base.Include1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include2(bool async)
    {
      return base.Include2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include3(bool async)
    {
      return base.Include3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include4(bool async)
    {
      return base.Include4(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include5(bool async)
    {
      return base.Include5(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include6(bool async)
    {
      return base.Include6(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include7(bool async)
    {
      return base.Include7(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include8(bool async)
    {
      return base.Include8(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include9(bool async)
    {
      return base.Include9(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include10(bool async)
    {
      return base.Include10(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include11(bool async)
    {
      return base.Include11(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include12(bool async)
    {
      return base.Include12(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include13(bool async)
    {
      return base.Include13(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include14(bool async)
    {
      return base.Include14(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include17(bool async)
    {
      return base.Include17(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include18_1(bool async)
    {
      return base.Include18_1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include18_1_1(bool async)
    {
      return base.Include18_1_1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include18_2(bool async)
    {
      return base.Include18_2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Include18_3(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Include18_3_1(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Include18_3_2(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include18_3_3(bool async)
    {
      return base.Include18_3_3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Include18_4(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Include18(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Include19(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include_with_all_method_include_gets_ignored(bool async)
    {
      return base.Include_with_all_method_include_gets_ignored(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_with_navigations_in_the_result_selector1(bool async)
    {
      return base.Join_with_navigations_in_the_result_selector1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Join_with_navigations_in_the_result_selector2(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Member_pushdown_chain_3_levels_deep(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Member_pushdown_chain_3_levels_deep_entity(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Member_pushdown_with_collection_navigation_in_the_middle(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Member_pushdown_with_multiple_collections(bool async)
    {
      return base.Member_pushdown_with_multiple_collections(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Include_multiple_collections_on_same_level(bool async)
    {
      return base.Include_multiple_collections_on_same_level(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Null_check_removal_applied_recursively(bool async)
    {
      return base.Null_check_removal_applied_recursively(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Null_check_different_structure_does_not_remove_null_checks(bool async)
    {
      return base.Null_check_different_structure_does_not_remove_null_checks(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Union_over_entities_with_different_nullability(bool async)
    {
      return base.Union_over_entities_with_different_nullability(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Including_reference_navigation_and_projecting_collection_navigation_2(bool async)
    {
      return base.Including_reference_navigation_and_projecting_collection_navigation_2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task OrderBy_collection_count_ThenBy_reference_navigation(bool async)
    {
      return base.OrderBy_collection_count_ThenBy_reference_navigation(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Null_conditional_is_not_applied_explicitly_for_optional_navigation(bool async)
    {
      return base.Null_conditional_is_not_applied_explicitly_for_optional_navigation(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Sum_with_selector_cast_using_as(bool async)
    {
      return base.Sum_with_selector_cast_using_as(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Sum_with_filter_with_include_selector_cast_using_as(bool async)
    {
      return base.Sum_with_filter_with_include_selector_cast_using_as(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_with_joined_where_clause_cast_using_as(bool async)
    {
      return base.Select_with_joined_where_clause_cast_using_as(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_with_outside_reference_to_joined_table_correctly_translated_to_apply(bool async)
    {
      return base.SelectMany_with_outside_reference_to_joined_table_correctly_translated_to_apply(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async)
    {
      return base.Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Contains_over_optional_navigation_with_null_constant(bool async)
    {
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Contains_over_optional_navigation_with_null_parameter(bool async)
    {
      return base.Contains_over_optional_navigation_with_null_parameter(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Contains_over_optional_navigation_with_null_column(bool async)
    {
      return base.Contains_over_optional_navigation_with_null_column(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Contains_over_optional_navigation_with_null_entity_reference(bool async)
    {
      return base.Contains_over_optional_navigation_with_null_entity_reference(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Element_selector_with_coalesce_repeated_in_aggregate(bool async)
    {
      return base.Element_selector_with_coalesce_repeated_in_aggregate(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Nested_object_constructed_from_group_key_properties(bool async)
    {
      return base.Nested_object_constructed_from_group_key_properties(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupBy_aggregate_where_required_relationship(bool async)
    {
      return base.GroupBy_aggregate_where_required_relationship(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupBy_aggregate_where_required_relationship_2(bool async)
    {
      return base.GroupBy_aggregate_where_required_relationship_2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Member_over_null_check_ternary_and_nested_dto_type(bool async)
    {
      return base.Member_over_null_check_ternary_and_nested_dto_type(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Member_over_null_check_ternary_and_nested_anonymous_type(bool async)
    {
      return base.Member_over_null_check_ternary_and_nested_anonymous_type(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Distinct_skip_without_orderby(bool async)
    {
      return base.Distinct_skip_without_orderby(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Distinct_take_without_orderby(bool async)
    {
      return base.Distinct_take_without_orderby(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Let_let_contains_from_outer_let(bool async)
    {
      return base.Let_let_contains_from_outer_let(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_conditionals_in_projection(bool async)
    {
      return base.Multiple_conditionals_in_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Composite_key_join_on_groupby_aggregate_projecting_only_grouping_key(bool async)
    {
      return base.Composite_key_join_on_groupby_aggregate_projecting_only_grouping_key(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_joins_groupby_predicate(bool async)
    {
      return base.Multiple_joins_groupby_predicate(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Collection_FirstOrDefault_property_accesses_in_projection(bool async)
    {
      return base.Collection_FirstOrDefault_property_accesses_in_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Collection_FirstOrDefault_entity_reference_accesses_in_projection(bool async)
    {
      return base.Collection_FirstOrDefault_entity_reference_accesses_in_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Collection_FirstOrDefault_entity_collection_accesses_in_projection(bool async)
    {
      return base.Collection_FirstOrDefault_entity_collection_accesses_in_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Multiple_collection_FirstOrDefault_followed_by_member_access_in_projection(bool async)
    {
      return base.Multiple_collection_FirstOrDefault_followed_by_member_access_in_projection(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Projecting_columns_with_same_name_from_different_entities_making_sure_aliasing_works_after_Distinct(bool async)
    {
      return base.Projecting_columns_with_same_name_from_different_entities_making_sure_aliasing_works_after_Distinct(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Complex_query_with_let_collection_SelectMany(bool async)
    {
      return base.Complex_query_with_let_collection_SelectMany(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task SelectMany_without_collection_selector_returning_queryable(bool async)
    {
      return base.SelectMany_without_collection_selector_returning_queryable(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_projecting_queryable_followed_by_SelectMany(bool async)
    {
      return base.Select_projecting_queryable_followed_by_SelectMany(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Join_with_result_selector_returning_queryable_throws_validation_error(bool async)
    {
      return base.Join_with_result_selector_returning_queryable_throws_validation_error(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_projecting_queryable_followed_by_Join(bool async)
    {
      return base.Select_projecting_queryable_followed_by_Join(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Select_projecting_queryable_in_anonymous_projection_followed_by_Join(bool async)
    {
      return base.Select_projecting_queryable_in_anonymous_projection_followed_by_Join(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Project_shadow_properties1(bool async)
    {
      return base.Project_shadow_properties1(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Project_shadow_properties2(bool async)
    {
      return base.Project_shadow_properties2(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Project_shadow_properties3(bool async)
    {
      return base.Project_shadow_properties3(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Project_shadow_properties4(bool async)
    {
      return base.Project_shadow_properties4(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Project_shadow_properties5(bool async)
    {
      return base.Project_shadow_properties5(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Project_shadow_properties6(bool async)
    {
      return base.Project_shadow_properties6(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Project_shadow_properties7(bool async)
    {
      return base.Project_shadow_properties7(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Project_shadow_properties8(bool async)
    {
      return base.Project_shadow_properties8(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Project_shadow_properties9(bool async)
    {
      return base.Project_shadow_properties9(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Project_shadow_properties10(bool async)
    {
      return base.Project_shadow_properties10(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task Prune_does_not_throw_null_ref(bool async)
    {
      return base.Prune_does_not_throw_null_ref(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_SelectMany_DefaultIfEmpty_with_predicate_using_closure(bool async)
    {
      return base.GroupJoin_SelectMany_DefaultIfEmpty_with_predicate_using_closure(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_SelectMany_with_predicate_using_closure(bool async)
    {
      return base.GroupJoin_SelectMany_with_predicate_using_closure(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_SelectMany_DefaultIfEmpty_with_predicate_using_closure_nested(bool async)
    {
      return base.GroupJoin_SelectMany_DefaultIfEmpty_with_predicate_using_closure_nested(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_SelectMany_with_predicate_using_closure_nested(bool async)
    {
      return base.GroupJoin_SelectMany_with_predicate_using_closure_nested(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_SelectMany_DefaultIfEmpty_with_predicate_using_closure_nested_same_param(bool async)
    {
      return base.GroupJoin_SelectMany_DefaultIfEmpty_with_predicate_using_closure_nested_same_param(async);
    }

    [ConditionalTheory(Skip = "Test skipped because of an issue with MULTISTATEMENT. Bug: SNOW-1883691.")]
    [MemberData("IsAsyncData", new object[] {})]
    public override Task GroupJoin_SelectMany_with_predicate_using_closure_nested_same_param(bool async)
    {
      return base.GroupJoin_SelectMany_with_predicate_using_closure_nested_same_param(async);
    }
}