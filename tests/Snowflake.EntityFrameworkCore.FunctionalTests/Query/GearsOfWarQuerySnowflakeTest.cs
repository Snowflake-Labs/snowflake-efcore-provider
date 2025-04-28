using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Xunit.Abstractions;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public class GearsOfWarQuerySnowflakeTest : GearsOfWarQueryRelationalTestBase<GearsOfWarQuerySnowflakeFixture>
{
    public GearsOfWarQuerySnowflakeTest(GearsOfWarQuerySnowflakeFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Millisecond Issue

    [ConditionalTheory(Skip = "Skipped because DATE_PART function does not support Milliseconds part")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_TimeSpan_Milliseconds(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped because DATE_PART function does not support Milliseconds part")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_TimeOnly_Millisecond(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped because DATE_PART function does not support Milliseconds part")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_datetimeoffset_millisecond_component(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped because DATE_PART function does not support Milliseconds part")]
    [MemberData(nameof(IsAsyncData))]
    public override Task TimeSpan_Milliseconds(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped because DATE_PART function does not support Milliseconds part")]
    [MemberData(nameof(IsAsyncData))]
    public override Task DateTimeOffset_to_unix_time_milliseconds(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped because DATE_PART function does not support Milliseconds part")]
    [MemberData(nameof(IsAsyncData))]
    public override Task DateTimeOffset_DateAdd_AddMilliseconds(bool async) => Task.CompletedTask;

    #endregion

    #region APPLY Invalid Operation Exception

    public override async Task Correlated_collection_after_distinct_3_levels(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Correlated_collection_after_distinct_3_levels(async));

    public override async Task
        Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(async));

    public override async Task Correlated_collection_with_distinct_not_projecting_identifier_column(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Correlated_collection_with_distinct_not_projecting_identifier_column(async));

    public override async Task Correlated_collection_with_distinct_projecting_identifier_column(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Correlated_collection_with_distinct_projecting_identifier_column(async));

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(
            bool async) => await Assert.ThrowsAsync<InvalidOperationException>(() =>
        base
            .Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(
                async));

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
            bool async) => await Assert.ThrowsAsync<InvalidOperationException>(() =>
        base
            .Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
                async));

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(
            bool async) => await Assert.ThrowsAsync<InvalidOperationException>(() =>
        base
            .Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(
                async));

    public override async Task
        Correlated_collection_with_groupby_with_complex_grouping_key_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
            bool async) => await Assert.ThrowsAsync<InvalidOperationException>(() =>
        base
            .Correlated_collection_with_groupby_with_complex_grouping_key_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
                async));

    public override async Task
        Correlated_collection_with_inner_collection_references_element_two_levels_up(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Correlated_collection_with_inner_collection_references_element_two_levels_up(async));

    public override async Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Correlated_collections_inner_subquery_predicate_references_outer_qsre(async));

    public override async Task
        Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(async));

    public override async Task
        Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(async));

    public override async Task Correlated_collections_with_Distinct(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Correlated_collections_with_Distinct(async));

    public override async Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Outer_parameter_in_group_join_with_DefaultIfEmpty(async));

    public override async Task Outer_parameter_in_join_key(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Outer_parameter_in_join_key(async));

    public override async Task Outer_parameter_in_join_key_inner_and_outer(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Outer_parameter_in_join_key_inner_and_outer(async));

    public override async Task
        SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(async));

    public override async Task
        Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(async));

    public override async Task
        Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(
            bool async) => await Assert.ThrowsAsync<InvalidOperationException>(() =>
        base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(
            async));

    public override async Task
        Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(async));

    public override async Task
        Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(async));

    public override async Task
        Correlated_collections_inner_subquery_selector_references_outer_qsre(bool async) =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(async));

    #endregion

    #region Unsupported subquery
    
    [ConditionalTheory(Skip = "Query on Snowflake generate an exception when using null value.")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_boolean_empty(bool async) => Task.CompletedTask;
    
    [ConditionalTheory(Skip = "Query on Snowflake generate an exception when using null value.")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_distinct_singleordefault_boolean_empty2(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Conditional_expression_with_test_being_simplified_to_constant_complex(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Correlated_collections_with_FirstOrDefault(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Filter_on_subquery_projecting_one_value_type_from_empty_collection(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task FirstOrDefault_on_empty_collection_of_DateTime_in_subquery(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task FirstOrDefault_over_int_compared_to_zero(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Include_collection_with_complex_OrderBy2(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Include_collection_with_complex_OrderBy3(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Include_with_complex_order_by(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task
        Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(
            bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Nav_expansion_with_member_pushdown_inside_Contains_argument(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Optional_navigation_with_collection_composite_key(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Project_one_value_type_converted_to_nullable_from_empty_collection(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Project_one_value_type_from_empty_collection(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task
        Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_boolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_boolean_with_pushdown(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_distinct_firstordefault(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_distinct_singleordefault_boolean1(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_distinct_singleordefault_boolean2(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_distinct_singleordefault_boolean_with_pushdown(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_int_with_inside_cast_and_coalesce(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_int_with_outside_cast_and_coalesce(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_int_with_pushdown_and_coalesce(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_int_with_pushdown_and_coalesce2(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_projecting_single_constant_bool(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_projecting_single_constant_int(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_subquery_projecting_single_constant_string(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_contains_on_navigation_with_composite_keys(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_boolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_boolean_with_pushdown(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_concat_firstordefault_boolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_distinct_first_boolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_distinct_firstordefault_boolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_distinct_firstordefault_boolean_with_pushdown(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_distinct_last_boolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_distinct_lastordefault_boolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_distinct_orderby_firstordefault_boolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_distinct_singleordefault_boolean1(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_distinct_singleordefault_boolean2(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_distinct_singleordefault_boolean_with_pushdown(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_join_firstordefault_boolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_left_join_firstordefault_boolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_union_firstordefault_boolean(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_with_ElementAtOrDefault_equality_to_null_with_composite_key(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_subquery_with_ElementAt_using_column_as_index(bool async) => Task.CompletedTask;

    #endregion

    #region TO_BINARY Issues (SNOW-1858573)

    [ConditionalTheory(Skip = "Skipped due SNOW-1858573")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Byte_array_contains_literal(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1858573")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Byte_array_contains_parameter(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1858573")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Contains_on_byte_array_property_using_byte_column(bool async) => Task.CompletedTask;

    #endregion

    #region TO_NUMBER Issues (SNOW-1858650)

    [ConditionalTheory(Skip = "Skipped due SNOW-1858650")]
    [MemberData(nameof(IsAsyncData))]
    public override Task First_on_byte_array(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1858650")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Array_access_on_byte_array(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1858650")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Using_indexer_on_byte_array_and_string_in_projection(bool async) => Task.CompletedTask;

    #endregion

    #region LINQ expression could not be translated. (SNOW-1858799)

    [ConditionalTheory(Skip = "Skipped due SNOW-1858799")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_TimeOnly_Add_TimeSpan(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1858799")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_TimeOnly_subtract_TimeOnly(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1858799")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_DateOnly_DayOfWeek(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1858799")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Subquery_inside_Take_argument(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1858799")]
    [MemberData(nameof(IsAsyncData))]
    public override Task DateTimeOffsetNow_minus_timespan(bool async) => Task.CompletedTask;

    #endregion

    #region Bitwise Operator Issue (SNOW-1860533)

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Constant_enum_with_same_underlying_value_as_previously_parameterized_int(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Enum_flags_closure_typed_as_different_type_generates_correct_parameter_type(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Enum_flags_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Enum_matching_take_value_gets_different_type_mapping(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_bitwise_and_enum(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_bitwise_and_integral(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_bitwise_and_nullable_enum_with_constant(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_bitwise_and_nullable_enum_with_nullable_parameter(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Bitwise_operation_with_non_null_parameter_optimizes_null_checks(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Bitwise_operation_with_null_arguments(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Bitwise_projects_values_in_select(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_with_enum_flags_parameter(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_has_flag_with_nullable_parameter(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_enum_has_flag_with_non_nullable_parameter(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_enum_has_flag(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_enum_has_flag_subquery_client_eval(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_enum_has_flag_subquery_with_pushdown(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_enum_has_flag_subquery(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1860533")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Select_enum_has_flag(bool async) => Task.CompletedTask;

    #endregion

    #region Internal Error (SNOW-1890097)

    [ConditionalTheory(Skip = "Skipped due SNOW-1890097")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Include_after_SelectMany_throws(bool async)
        => Task.CompletedTask;

    #endregion

    public override async Task Enum_ToString_is_client_eval(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Gears.OrderBy(g => g.SquadId)
            .ThenBy(g => g.Nickname)
            .Select(g => g.Rank);

        var results = query.ToList();

        results.ForEach(r => Assert.Equal(r.ToString(), r.ToString()));
    }

    public override async Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
    {
        var dto = new DateTimeOffset(599898024001234567, new TimeSpan(0));
        var start = dto.AddDays(-1);
        var end = dto.AddDays(1);
        var dates = new[] { dto };

        await AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(
                m => start <= m.Timeline.Date && m.Timeline < end && dates.Contains(m.Timeline)),
            assertEmpty: true);

        AssertSql(
            """
            __start_0='1902-01-01T10:00:00.1234567+00:00'
            __end_1='1902-01-03T10:00:00.1234567+00:00'

            SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
            FROM "Missions" AS "m"
            WHERE :__start_0 <= CAST(CAST("m"."Timeline" AS DATE) AS timestamp_tz) AND "m"."Timeline" < :__end_1 AND "m"."Timeline" = '1902-01-02 10:00:00.1234567+00:00'
            """);
    }

    public override Task ToString_boolean_property_non_nullable(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(w => w.IsAutomatic.ToString().ToLower()));

    public override Task ToString_boolean_property_nullable(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustHorde>().Select(lh => lh.Eradicated.ToString()!.ToLower()));

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}