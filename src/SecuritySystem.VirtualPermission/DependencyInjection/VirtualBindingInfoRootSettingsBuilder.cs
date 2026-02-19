using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SecuritySystem.DependencyInjection;
using SecuritySystem.ExternalSystem;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public class VirtualBindingInfoRootSettingsBuilder<TPrincipal, TPermission> : IVirtualBindingInfoRootSettingsBuilder<TPermission>
    where TPermission : class
{
    private readonly List<VirtualPermissionBindingInfo<TPermission>> virtualPermissionBindingInfoList = [];

    private readonly List<Func<PermissionBindingInfo<TPermission, TPrincipal>, PermissionBindingInfo<TPermission, TPrincipal>>> permissionBindingInit = [];

    public IVirtualBindingInfoRootSettingsBuilder<TPermission> SetPeriod(
        PropertyAccessors<TPermission, DateTime?>? startDatePropertyAccessor,
        PropertyAccessors<TPermission, DateTime?>? endDatePropertyAccessor)
    {
        this.permissionBindingInit.Add(permissionBinding => permissionBinding with { PermissionStartDate = startDatePropertyAccessor });
        this.permissionBindingInit.Add(permissionBinding => permissionBinding with { PermissionEndDate = endDatePropertyAccessor });

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
        this.permissionBindingInit.Add(permissionBinding => permissionBinding with { PermissionComment = commentPath.ToPropertyAccessors() });

        return this;
    }

    public IVirtualBindingInfoRootSettingsBuilder<TPermission> SetPermissionDelegation(
        Expression<Func<TPermission, TPermission?>> newDelegatedFromPath)
    {
        this.permissionBindingInit.Add(permissionBinding => permissionBinding with { DelegatedFrom = newDelegatedFromPath.ToPropertyAccessors() });

        return this;
    }

    public IVirtualBindingInfoRootSettingsBuilder<TPermission> ForRole(SecurityRole securityRole,
        Action<IVirtualBindingInfoSettingsBuilder<TPermission>>? init = null)
    {
        var innerBuilder = new VirtualBindingInfoSettingsBuilder<TPermission>();

        init?.Invoke(innerBuilder);

        var virtualBindingInfo = innerBuilder.Init(new VirtualPermissionBindingInfo<TPermission> { SecurityRole = securityRole });

        this.virtualPermissionBindingInfoList.Add(virtualBindingInfo);

        return this;
    }

    public void Initialize(ISecuritySystemSettings securitySystemSettings, PropertyAccessors<TPermission, TPrincipal> principalAccessors)
    {
        var baseBindingInfo = new PermissionBindingInfo<TPermission, TPrincipal> { IsReadonly = true, Principal = principalAccessors };

        var bindingInfo = permissionBindingInit.Aggregate(baseBindingInfo, (state, f) => f(state));

        foreach (var virtualBindingInfo in this.virtualPermissionBindingInfoList)
        {
            securitySystemSettings.AddPermissionSystem(serviceProxyFactory =>
                serviceProxyFactory.Create<IPermissionSystemFactory, VirtualPermissionSystemFactory>(virtualBindingInfo));
        }

        securitySystemSettings.AddExtensions(services =>
        {
            services.AddSingleton<PermissionBindingInfo>(bindingInfo);

            services.TryAddSingleton<IVirtualPermissionBindingInfoValidator, VirtualPermissionBindingInfoValidator>();

            foreach (var virtualBindingInfo in this.virtualPermissionBindingInfoList)
            {
                services.AddScopedFrom<IPrincipalSourceService, IServiceProxyFactory>(factory =>
                    factory.Create<IPrincipalSourceService, VirtualPrincipalSourceService<TPermission>>(virtualBindingInfo));
            }
        });
    }
}