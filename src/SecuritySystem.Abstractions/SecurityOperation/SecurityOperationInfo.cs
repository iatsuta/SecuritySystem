

// ReSharper disable once CheckNamespace
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem;

public record SecurityOperationInfo
{
    public HierarchicalExpandType? CustomExpandType { get; init; } = null;

    public string? Description { get; init; } = null;
}
