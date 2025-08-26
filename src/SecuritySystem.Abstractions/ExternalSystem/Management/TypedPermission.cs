namespace SecuritySystem.ExternalSystem.Management;

public record TypedPermission(
    Guid Id,
    bool IsVirtual,
    SecurityRole SecurityRole,
    DateTime StartDate,
    DateTime? EndDate,
    string Comment,
    IReadOnlyDictionary<Type, Array> Restrictions);
