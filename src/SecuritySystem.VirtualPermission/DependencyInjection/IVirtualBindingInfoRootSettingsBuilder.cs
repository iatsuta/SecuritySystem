using CommonFramework;

using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public interface IVirtualPermissionRootBuilder<TPermission>
    where TPermission : notnull
{
    IVirtualPermissionRootBuilder<TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, IEnumerable<TSecurityContext>>> path)
        where TSecurityContext : ISecurityContext;

    IVirtualPermissionRootBuilder<TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, TSecurityContext?>> path)
        where TSecurityContext : ISecurityContext;

    IVirtualPermissionRootBuilder<TPermission> SetPeriod(
        PropertyAccessors<TPermission, DateTime?>? startDatePropertyAccessor,
        PropertyAccessors<TPermission, DateTime?>? endDatePropertyAccessor);

    IVirtualPermissionRootBuilder<TPermission> SetPeriod(
        Expression<Func<TPermission, DateTime?>>? startDatePath,
        Expression<Func<TPermission, DateTime?>>? endDatePath);

    IVirtualPermissionRootBuilder<TPermission> SetComment(Expression<Func<TPermission, string>> commentPath);

    IVirtualPermissionRootBuilder<TPermission> SetPermissionDelegation(Expression<Func<TPermission, TPermission?>> newDelegatedFromPath);

    IVirtualPermissionRootBuilder<TPermission> AddSecurityRole(SecurityRole securityRole, Action<IVirtualBindingInfoSettingsBuilder<TPermission>>? init = null);
}