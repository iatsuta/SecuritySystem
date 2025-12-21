namespace SecuritySystem.ExternalSystem.Management;

public record ManagedPermission(
    SecurityIdentity Identity,
    bool IsVirtual,
    SecurityRole SecurityRole,
    PermissionPeriod Period,
    string Comment,
    IReadOnlyDictionary<Type, Array> Restrictions);
