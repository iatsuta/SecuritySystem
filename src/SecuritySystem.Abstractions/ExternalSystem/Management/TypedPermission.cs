namespace SecuritySystem.ExternalSystem.Management;

public record TypedPermission(
    string Id,
    bool IsVirtual,
    SecurityRole SecurityRole,
    DateTime StartDate,
    DateTime? EndDate,
    string Comment,
    IReadOnlyDictionary<Type, Array> Restrictions);
