using Snowflake.EntityFrameworkCore.Metadata.Internal;

namespace Snowflake.EntityFrameworkCore.Migrations.Internal;

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

public class SnowflakeMigrationsAnnotationProvider : MigrationsAnnotationProvider
{
    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
#pragma warning disable EF1001 // Internal EF Core API usage.
    public SnowflakeMigrationsAnnotationProvider(MigrationsAnnotationProviderDependencies dependencies)
#pragma warning restore EF1001 // Internal EF Core API usage.
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRemove(IRelationalModel model)
        => model.GetAnnotations();

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRemove(ITable table)
        => table.GetAnnotations();

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRemove(IUniqueConstraint constraint)
    {
        if (constraint.Table[SnowflakeAnnotationNames.IsTemporal] as bool? == true)
        {
            yield return new Annotation(SnowflakeAnnotationNames.IsTemporal, true);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRemove(IColumn column)
    {
        if (column.Table[SnowflakeAnnotationNames.IsTemporal] as bool? == true)
        {
            yield return new Annotation(SnowflakeAnnotationNames.IsTemporal, true);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRename(ITable table)
    {
        if (table[SnowflakeAnnotationNames.IsTemporal] as bool? == true)
        {
            yield return new Annotation(SnowflakeAnnotationNames.IsTemporal, true);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRename(IColumn column)
    {
        if (column.Table[SnowflakeAnnotationNames.IsTemporal] as bool? == true)
        {
            yield return new Annotation(SnowflakeAnnotationNames.IsTemporal, true);
        }
    }
}
