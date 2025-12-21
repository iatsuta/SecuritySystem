using CommonFramework;

namespace SecuritySystem;

public abstract record PermissionBindingInfo
{
    public bool IsReadonly { get; init; }

    public abstract Type PrincipalType { get; }

    public abstract Type PermissionType { get; }
}

public abstract record PermissionBindingInfo<TPermission> : PermissionBindingInfo
{
    public sealed override Type PermissionType { get; } = typeof(TPermission);

    public PropertyAccessors<TPermission, string>? PermissionComment { get; init; }

    public PropertyAccessors<TPermission, PermissionPeriod>? PermissionPeriod { get; init; }

    public PermissionPeriod GetSafePeriod(TPermission permission) =>
        this.PermissionPeriod == null ? global::SecuritySystem.PermissionPeriod.Eternity : this.PermissionPeriod.Getter(permission);

    public string GetSafeComment(TPermission permission) =>
        this.PermissionComment == null ? "" : this.PermissionComment.Getter(permission);
}

public record PermissionBindingInfo<TPermission, TPrincipal> : PermissionBindingInfo<TPermission>
{
    public sealed override Type PrincipalType { get; } = typeof(TPrincipal);

    public required PropertyAccessors<TPermission, TPrincipal> Principal { get; init; }
}