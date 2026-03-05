using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission;

public abstract record VirtualPermissionSecurityRoleItemBindingInfo
{
    public abstract Type PermissionType { get; }

    public required SecurityRole SecurityRole { get; init; }
}

public record VirtualPermissionSecurityRoleItemBindingInfo<TPermission> : VirtualPermissionSecurityRoleItemBindingInfo
{
    public override Type PermissionType { get; } = typeof(TPermission);


    public Func<IServiceProvider, Expression<Func<TPermission, bool>>> Filter { get; init; } = _ => _ => true;
}