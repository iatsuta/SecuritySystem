using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Services;

public class AvailablePrincipalSource<TPrincipal>(
    IServiceProvider serviceProvider,
    IPermissionBindingInfoSource bindingInfoSource) : IAvailablePrincipalSource<TPrincipal>
{
    private readonly Lazy<IAvailablePrincipalSource<TPrincipal>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPrincipal));

        var innerServiceType = typeof(AvailablePrincipalSource<,>).MakeGenericType(bindingInfo.PrincipalType, bindingInfo.PermissionType);

        return (IAvailablePrincipalSource<TPrincipal>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            bindingInfo);
    });

    public IQueryable<TPrincipal> GetAvailablePrincipalsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        lazyInnerService.Value.GetAvailablePrincipalsQueryable(securityRule);
}

public class AvailablePrincipalSource<TPrincipal, TPermission>(
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    IAvailablePermissionSource<TPermission> availablePermissionSource) : IAvailablePrincipalSource<TPrincipal>
{
    public IQueryable<TPrincipal> GetAvailablePrincipalsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        return availablePermissionSource.GetQueryable(securityRule).Select(bindingInfo.Principal.Path).Distinct();
    }
}