using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public interface IVirtualBindingInfoRootSettingsBuilder<TPermission>
    where TPermission : notnull
{
    IVirtualBindingInfoRootSettingsBuilder<TPermission> SetPeriod(
        PropertyAccessors<TPermission, DateTime?>? startDatePropertyAccessor,
        PropertyAccessors<TPermission, DateTime?>? endDatePropertyAccessor);

     IVirtualBindingInfoRootSettingsBuilder<TPermission> SetPeriod(
        Expression<Func<TPermission, DateTime?>>? startDatePath,
        Expression<Func<TPermission, DateTime?>>? endDatePath);

    IVirtualBindingInfoRootSettingsBuilder<TPermission> SetComment(Expression<Func<TPermission, string>> commentPath);

    IVirtualBindingInfoRootSettingsBuilder<TPermission> ForRole(SecurityRole securityRole, Action<IVirtualBindingInfoSettingsBuilder<TPermission>>? init = null);
}