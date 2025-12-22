using CommonFramework;

using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public class VirtualBindingInfoRootSettingsBuilder<TPermission> : IVirtualBindingInfoRootSettingsBuilder<TPermission>
    where TPermission : notnull
{
    public readonly List<VirtualPermissionBindingInfo<TPermission>> VirtualPermissionBindingInfoList = new();

    public readonly List<Func<PermissionBindingInfo<TPermission>, PermissionBindingInfo<TPermission>>> PermissionBindingInit = new();

    public IVirtualBindingInfoRootSettingsBuilder<TPermission> SetPeriod(
        PropertyAccessors<TPermission, DateTime?>? startDatePropertyAccessor,
        PropertyAccessors<TPermission, DateTime?>? endDatePropertyAccessor)
    {
        this.PermissionBindingInit.Add(permissionBinding => permissionBinding with { PermissionStartDate = startDatePropertyAccessor });
        this.PermissionBindingInit.Add(permissionBinding => permissionBinding with { PermissionEndDate = endDatePropertyAccessor });

        return this;
    }

    public IVirtualBindingInfoRootSettingsBuilder<TPermission> SetPeriod(
        Expression<Func<TPermission, DateTime?>>? startDatePath,
        Expression<Func<TPermission, DateTime?>>? endDatePath) =>
        this.SetPeriod(
            startDatePath == null ? null : new PropertyAccessors<TPermission, DateTime?>(startDatePath),
            endDatePath == null ? null : new PropertyAccessors<TPermission, DateTime?>(endDatePath));

    public IVirtualBindingInfoRootSettingsBuilder<TPermission> SetComment(Expression<Func<TPermission, string>> commentPath)
    {
        this.PermissionBindingInit.Add(permissionBinding => permissionBinding with { PermissionComment = commentPath.ToPropertyAccessors() });

        return this;
    }

    public IVirtualBindingInfoRootSettingsBuilder<TPermission> ForRole(SecurityRole securityRole,
        Action<IVirtualBindingInfoSettingsBuilder<TPermission>>? init = null)
    {
        var innerBuilder = new VirtualBindingInfoSettingsBuilder<TPermission>();

        init?.Invoke(innerBuilder);

        var virtualBindingInfo = innerBuilder.Init(new VirtualPermissionBindingInfo<TPermission> { SecurityRole = securityRole });

        this.VirtualPermissionBindingInfoList.Add(virtualBindingInfo);

        return this;
    }
}