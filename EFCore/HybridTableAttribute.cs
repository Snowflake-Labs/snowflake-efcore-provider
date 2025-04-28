using System;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Marks a type as a Standard Snowflake Table.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class HybridTableAttribute : Attribute
{
}
