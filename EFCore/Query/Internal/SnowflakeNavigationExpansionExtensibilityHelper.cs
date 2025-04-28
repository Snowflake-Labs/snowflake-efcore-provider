using Snowflake.EntityFrameworkCore.Internal;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <inheritdoc />
public class SnowflakeNavigationExpansionExtensibilityHelper : NavigationExpansionExtensibilityHelper
{
    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeNavigationExpansionExtensibilityHelper" />
    /// </summary>
    /// <param name="dependencies"></param>
    public SnowflakeNavigationExpansionExtensibilityHelper(
        NavigationExpansionExtensibilityHelperDependencies dependencies)
        : base(dependencies)
    {
    }


    /// <summary>
    ///   Creates a new query root expression.
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public override EntityQueryRootExpression CreateQueryRoot(IEntityType entityType, EntityQueryRootExpression? source)
    {
        if (source is TemporalAsOfQueryRootExpression asOf)
        {
            // AsOf is the only temporal operation that can pass the validation
            return source.QueryProvider != null
                ? new TemporalAsOfQueryRootExpression(source.QueryProvider, entityType, asOf.PointInTime)
                : new TemporalAsOfQueryRootExpression(entityType, asOf.PointInTime);
        }

        return base.CreateQueryRoot(entityType, source);
    }

    /// <summary>
    ///  Validates the creation of a query root expression.
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="source"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public override void ValidateQueryRootCreation(IEntityType entityType, EntityQueryRootExpression? source)
    {
        if (source is TemporalQueryRootExpression
            && !entityType.IsMappedToJson()
            && !OwnedEntityMappedToSameTableAsOwner(entityType))
        {
            if (!entityType.GetRootType().IsTemporal())
            {
                throw new InvalidOperationException(
                    SnowflakeStrings.TemporalNavigationExpansionBetweenTemporalAndNonTemporal(entityType.DisplayName()));
            }

            if (source is not TemporalAsOfQueryRootExpression)
            {
                throw new InvalidOperationException(
                    SnowflakeStrings.TemporalNavigationExpansionOnlySupportedForAsOf("AsOf"));
            }
        }

        base.ValidateQueryRootCreation(entityType, source);
    }

    private bool OwnedEntityMappedToSameTableAsOwner(IEntityType entityType)
        => entityType.IsOwned()
            && entityType.FindOwnership()!.PrincipalEntityType.GetTableMappings().FirstOrDefault()?.Table is ITable ownerTable
                && entityType.GetTableMappings().FirstOrDefault()?.Table is ITable entityTable
                && ownerTable == entityTable;

    /// <summary>
    /// Validates the compatibility of two query roots.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override bool AreQueryRootsCompatible(EntityQueryRootExpression? first, EntityQueryRootExpression? second)
    {
        if (!base.AreQueryRootsCompatible(first, second))
        {
            return false;
        }

        var firstTemporal = first is TemporalQueryRootExpression;
        var secondTemporal = second is TemporalQueryRootExpression;

        if (firstTemporal && secondTemporal)
        {
            if (first is TemporalAsOfQueryRootExpression firstAsOf
                && second is TemporalAsOfQueryRootExpression secondAsOf
                && firstAsOf.PointInTime == secondAsOf.PointInTime)
            {
                return true;
            }

            if (first is TemporalAllQueryRootExpression
                && second is TemporalAllQueryRootExpression)
            {
                return true;
            }

            if (first is TemporalRangeQueryRootExpression firstRange
                && second is TemporalRangeQueryRootExpression secondRange
                && firstRange.From == secondRange.From
                && firstRange.To == secondRange.To)
            {
                return true;
            }
        }

        if (firstTemporal || secondTemporal)
        {
            var entityType = first?.EntityType ?? second?.EntityType;

            throw new InvalidOperationException(SnowflakeStrings.TemporalSetOperationOnMismatchedSources(entityType!.DisplayName()));
        }

        return true;
    }
}