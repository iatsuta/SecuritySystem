using CommonFramework;
using CommonFramework.IdentitySource;

using SecuritySystem.Services;

using System.Linq.Expressions;
using CommonFramework.DependencyInjection;

namespace SecuritySystem.GeneralPermission;

public class PermissionSecurityRoleFilterFactory<TPermission>(
    IServiceProxyFactory serviceProxyFactory,
    IGeneralPermissionBindingInfoSource bindingInfoSource,
    IIdentityInfoSource identityInfoSource) : IPermissionSecurityRoleFilterFactory<TPermission>
{
    private readonly Lazy<IPermissionSecurityRoleFilterFactory<TPermission>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var securityRoleIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.SecurityRoleType);

        var innerServiceType = typeof(PermissionSecurityRoleFilterFactory<,,>).MakeGenericType(
            bindingInfo.PermissionType,
            bindingInfo.SecurityRoleType,
            securityRoleIdentityInfo.IdentityType);

        return serviceProxyFactory.Create<IPermissionSecurityRoleFilterFactory<TPermission>>(
            innerServiceType,
            bindingInfo,
            securityRoleIdentityInfo);
    });

    public Expression<Func<TPermission, bool>> CreateFilter(Type identType, Array idents) =>
        this.lazyInnerService.Value.CreateFilter(identType, idents);
}

public class PermissionSecurityRoleFilterFactory<TPermission, TSecurityRole, TSecurityRoleIdent>(
    GeneralPermissionBindingInfo<TPermission, TSecurityRole> bindingInfo,
    ISecurityIdentityConverter<TSecurityRoleIdent> securityIdentityConverter,
    IdentityInfo<TSecurityRole, TSecurityRoleIdent> identityInfo) : IPermissionSecurityRoleFilterFactory<TPermission>
    where TSecurityRoleIdent : notnull
{
    public Expression<Func<TPermission, bool>> CreateFilter(Type identType, Array idents)
    {
        return new Func<Ignore[], Expression<Func<TPermission, bool>>>(this.CreateFilter)
            .CreateGenericMethod(identType)
            .Invoke<Expression<Func<TPermission, bool>>>(this, idents);
    }

    private Expression<Func<TPermission, bool>> CreateFilter<TIdent>(TIdent[] idents)
        where TIdent : notnull
    {
        var convertedIdents = idents.Select(ident => securityIdentityConverter.Convert(TypedSecurityIdentity.Create(ident)).Id).ToList();

        var containsFilter = identityInfo.CreateContainsFilter(convertedIdents);

        return bindingInfo.SecurityRole.Path.Select(containsFilter);
    }
}