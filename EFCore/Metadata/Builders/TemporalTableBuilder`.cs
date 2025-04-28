using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Snowflake.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
/// <typeparam name="TEntity">The entity type being configured.</typeparam>
public class TemporalTableBuilder<TEntity> : TemporalTableBuilder
    where TEntity : class
{
    /// <summary>
    ///    Initializes a new instance of the <see cref="TemporalTableBuilder{TEntity}" /> class.
    /// </summary>
    /// <param name="entityTypeBuilder"></param>
    [EntityFrameworkInternal]
    public TemporalTableBuilder(EntityTypeBuilder entityTypeBuilder)
        : base(entityTypeBuilder)
    {
    }
}
