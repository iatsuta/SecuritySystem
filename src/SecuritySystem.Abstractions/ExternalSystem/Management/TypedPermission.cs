namespace SecuritySystem.ExternalSystem.Management;

public record TypedPermission(
    SecurityIdentity Identity,
    bool IsVirtual,
    SecurityRole SecurityRole,
    (DateTime StartDate, DateTime? EndDate) Period,
    string Comment,
    IReadOnlyDictionary<Type, Array> Restrictions);
