using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public interface IVirtualBindingInfoRootSettingsBuilder<TPermission>
    where TPermission : notnull
{
    IVirtualBindingInfoRootSettingsBuilder<TPermission> SetPeriod(Expression<Func<TPermission, PermissionPeriod>> periodPath,
        Action<TPermission, PermissionPeriod>? setter = null);

    IVirtualBindingInfoRootSettingsBuilder<TPermission> SetComment(Expression<Func<TPermission, string>> commentPath);

    IVirtualBindingInfoRootSettingsBuilder<TPermission> ForRole(SecurityRole securityRole, Action<IVirtualBindingInfoSettingsBuilder<TPermission>>? init = null);
}