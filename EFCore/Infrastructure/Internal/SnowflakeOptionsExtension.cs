using System.Text;

namespace Snowflake.EntityFrameworkCore.Infrastructure.Internal;

using System.Collections.Generic;
using Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

public class SnowflakeOptionsExtension : RelationalOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;
    private EnvironmentType _environmentType = Extensions.EnvironmentType.Unknown;

    /// <summary>
    ///     True if reverse null ordering is enabled; otherwise, false.
    /// </summary>
    public virtual bool ReverseNullOrdering { get; private set; }

    
    public SnowflakeOptionsExtension()
    {
    }

    // NB: When adding new options, make sure to update the copy ctor below.

    
    protected SnowflakeOptionsExtension(SnowflakeOptionsExtension copyFrom)
        : base(copyFrom)
    {
        ReverseNullOrdering = copyFrom.ReverseNullOrdering;
    }

    /// <summary>
    ///     Returns a copy of the current instance configured with the specified value..
    /// </summary>
    /// <param name="reverseNullOrdering">True to enable reverse null ordering; otherwise, false.</param>
    internal virtual SnowflakeOptionsExtension WithReverseNullOrdering(bool reverseNullOrdering)
    {
        var clone = (SnowflakeOptionsExtension)Clone();

        clone.ReverseNullOrdering = reverseNullOrdering;

        return clone;
    }

    /// <inheritdoc />
    public override  DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    /// <inheritdoc />
    protected override  RelationalOptionsExtension Clone()
        => new SnowflakeOptionsExtension(this);

    /// <inheritdoc />
    public override  void ApplyServices(IServiceCollection services)
        => services.AddEntityFrameworkSnowflake();

    private sealed class ExtensionInfo : RelationalExtensionInfo
    {
        private string? _logFragment;

        public ExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        private new SnowflakeOptionsExtension Extension
            => (SnowflakeOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider
            => true;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo;

        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    builder.Append(base.LogFragment);
                    builder.Append($"{Extension.EnvironmentType}Environment ");

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            => debugInfo["Snowflake"] = "1";
    }

    public SnowflakeOptionsExtension WithEnvironment(EnvironmentType environmentType)
    {
        _environmentType = environmentType;
        return this;
    }

    public EnvironmentType EnvironmentType => _environmentType;
}
