

// ReSharper disable once CheckNamespace
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem;

public record SecurityRoleInfo(SecurityIdentity Identity)
{
    public HierarchicalExpandType? CustomExpandType { get; init; } = null;

    public SecurityPathRestriction Restriction { get; init; } = SecurityPathRestriction.Default;

    public IReadOnlyList<SecurityOperation> Operations { get; init; } = [];

    public IReadOnlyList<SecurityRole> Children { get; init; } = [];

    public string? Description { get; init; }

    public bool IsVirtual { get; init; }
}
