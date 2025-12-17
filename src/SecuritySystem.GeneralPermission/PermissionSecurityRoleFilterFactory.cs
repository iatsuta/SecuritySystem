using CommonFramework;
using CommonFramework.IdentitySource;

using SecuritySystem.Services;

using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.GeneralPermission;

public class PermissionSecurityRoleFilterFactory<TPermission>(
    IServiceProvider serviceProvider,
    GeneralPermissionBindingInfo bindingInfo,
    IIdentityInfoSource identityInfoSource) : IPermissionSecurityRoleFilterFactory<TPermission>
{
    private readonly Lazy<IPermissionSecurityRoleFilterFactory<TPermission>> lazyInnerService = new(() =>
    {
        var securityRoleIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.SecurityRoleType);

        var innerServiceType = typeof(PermissionSecurityRoleFilterFactory<,,,>).MakeGenericType(
            bindingInfo.PrincipalType,
            typeof(TPermission),
            bindingInfo.SecurityRoleType,
            securityRoleIdentityInfo.IdentityType);

        return (IPermissionSecurityRoleFilterFactory<TPermission>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            securityRoleIdentityInfo);
    });

    public Expression<Func<TPermission, bool>> CreateFilter(Type identType, Array idents) =>
        this.lazyInnerService.Value.CreateFilter(identType, idents);
}

public class PermissionSecurityRoleFilterFactory<TPrincipal, TPermission, TSecurityRole, TSecurityRoleIdent>(
    ISecurityIdentityConverter<TSecurityRoleIdent> securityIdentityConverter,
    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole> permissionToSecurityRoleInfo,
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

        return permissionToSecurityRoleInfo.SecurityRole.Path.Select(identityInfo.Id.Path).Select(srId => convertedIdents.Contains(srId));
    }
}