using System.Collections.Immutable;

using CommonFramework;
using CommonFramework.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SecuritySystem.DependencyInjection;
using SecuritySystem.ExternalSystem;
using SecuritySystem.ExternalSystem.Management;

using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public class VirtualPermissionRootBuilder<TPrincipal, TPermission>(PropertyAccessors<TPermission, TPrincipal> principalAccessors) : IVirtualPermissionRootBuilder<TPermission>, IServiceInitializer<ISecuritySystemBuilder>
    where TPermission : class
{
    private readonly List<VirtualPermissionSecurityRoleItemBindingInfo<TPermission>> itemBindingInfoList = [];

    private readonly List<Func<PermissionBindingInfo<TPermission, TPrincipal>, PermissionBindingInfo<TPermission, TPrincipal>>> permissionBindingInit = [];

    private readonly List<Func<VirtualPermissionBindingInfo<TPermission>, VirtualPermissionBindingInfo<TPermission>>> virtualBindingInit = [];

    public IVirtualPermissionRootBuilder<TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, IEnumerable<TSecurityContext>>> path)
        where TSecurityContext : ISecurityContext
    {
        this.virtualBindingInit.Add(permissionBinding => permissionBinding with { Restrictions = permissionBinding.Restrictions.Concat([path]).ToImmutableList() });

        return this;
    }

    public IVirtualPermissionRootBuilder<TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, TSecurityContext?>> path)
        where TSecurityContext : ISecurityContext
    {
        this.virtualBindingInit.Add(permissionBinding => permissionBinding with { Restrictions = permissionBinding.Restrictions.Concat([path]).ToImmutableList() });

        return this;
    }

    public IVirtualPermissionRootBuilder<TPermission> SetPeriod(
        PropertyAccessors<TPermission, DateTime?>? startDatePropertyAccessor,
        PropertyAccessors<TPermission, DateTime?>? endDatePropertyAccessor)
    {
        this.permissionBindingInit.Add(permissionBinding => permissionBinding with { PermissionStartDate = startDatePropertyAccessor });
        this.permissionBindingInit.Add(permissionBinding => permissionBinding with { PermissionEndDate = endDatePropertyAccessor });

        return this;
    }

    public IVirtualPermissionRootBuilder<TPermission> SetPeriod(
        Expression<Func<TPermission, DateTime?>>? startDatePath,
        Expression<Func<TPermission, DateTime?>>? endDatePath) =>
        this.SetPeriod(
            startDatePath == null ? null : new PropertyAccessors<TPermission, DateTime?>(startDatePath),
            endDatePath == null ? null : new PropertyAccessors<TPermission, DateTime?>(endDatePath));

    public IVirtualPermissionRootBuilder<TPermission> SetComment(Expression<Func<TPermission, string>> commentPath)
    {
        this.permissionBindingInit.Add(permissionBinding => permissionBinding with { PermissionComment = commentPath.ToPropertyAccessors() });

        return this;
    }

    public IVirtualPermissionRootBuilder<TPermission> SetPermissionDelegation(
        Expression<Func<TPermission, TPermission?>> newDelegatedFromPath)
    {
        this.permissionBindingInit.Add(permissionBinding => permissionBinding with { DelegatedFrom = newDelegatedFromPath.ToPropertyAccessors() });

        return this;
    }

    public IVirtualPermissionRootBuilder<TPermission> AddSecurityRole(SecurityRole securityRole,
        Action<IVirtualBindingInfoSettingsBuilder<TPermission>>? init = null)
    {
        var innerBuilder = new VirtualBindingInfoSettingsBuilder<TPermission>();

        init?.Invoke(innerBuilder);

        var virtualBindingInfo = innerBuilder.Initialize(new VirtualPermissionSecurityRoleItemBindingInfo<TPermission> { SecurityRole = securityRole });

        this.itemBindingInfoList.Add(virtualBindingInfo);

        return this;
    }

    public void Initialize(ISecuritySystemBuilder securitySystemBuilder)
    {
        var bindingInfo =
            permissionBindingInit.Aggregate(new PermissionBindingInfo<TPermission, TPrincipal> { IsReadonly = true, Principal = principalAccessors },
                (state, f) => f(state));

        var virtualBindingInfo = virtualBindingInit.Aggregate(new VirtualPermissionBindingInfo<TPermission> { Items = itemBindingInfoList.ToImmutableList() },
            (state, f) => f(state));

        securitySystemBuilder.AddPermissionSystem(serviceProxyFactory =>
            serviceProxyFactory.Create<IPermissionSystemFactory, VirtualPermissionSystemFactory>(virtualBindingInfo));

        securitySystemBuilder.AddExtensions(services =>
        {
            services.AddSingleton<PermissionBindingInfo>(bindingInfo);

            services.TryAddSingleton<IVirtualPermissionBindingInfoValidator, VirtualPermissionBindingInfoValidator>();

            services.AddScopedFrom<IPrincipalSourceService, IServiceProxyFactory>(factory =>
                factory.Create<IPrincipalSourceService, VirtualPrincipalSourceService<TPermission>>(virtualBindingInfo));
        });
    }
}