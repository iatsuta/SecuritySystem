using CommonFramework;

namespace SecuritySystem.GeneralPermission;

public record GeneralPermissionBindingInfo<TPermission, TSecurityRole> : GeneralPermissionBindingInfo
{
    public sealed override Type PermissionType { get; } = typeof(TPermission);

    public sealed override Type SecurityRoleType { get; } = typeof(TSecurityRole);

    public required PropertyAccessors<TPermission, TSecurityRole> SecurityRole { get; init; }

    public PropertyAccessors<TSecurityRole, string>? SecurityRoleDescription { get; init; }
}

public abstract record GeneralPermissionBindingInfo
{
    public abstract Type PermissionType { get; }

    public abstract Type SecurityRoleType { get; }
}