using CommonFramework;

using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public class VirtualBindingInfoRootSettingsBuilder<TPermission> : IVirtualBindingInfoRootSettingsBuilder<TPermission>
    where TPermission : notnull
{
    public readonly List<VirtualPermissionBindingInfo<TPermission>> VirtualPermissionBindingInfoList = new();

    public readonly List<Func<PermissionBindingInfo<TPermission>, PermissionBindingInfo<TPermission>>> PermissionBindingInit = new();

    public IVirtualBindingInfoRootSettingsBuilder<TPermission> SetPeriod(Expression<Func<TPermission, PermissionPeriod>> periodPath,
        Action<TPermission, PermissionPeriod>? setter = null)
    {
        this.PermissionBindingInit.Add(permissionBinding =>
        {
            var propertyAccessors = setter == null
                ? periodPath.ToPropertyAccessors()
                : new PropertyAccessors<TPermission, PermissionPeriod>(periodPath, periodPath.Compile(), setter);

            return permissionBinding with { PermissionPeriod = propertyAccessors };
        });

        return this;
    }

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