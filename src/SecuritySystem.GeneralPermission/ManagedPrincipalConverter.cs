using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public class ManagedPrincipalConverter<TPrincipal>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    IGeneralPermissionBindingInfoSource bindingInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IManagedPrincipalConverter<TPrincipal>
{
    private readonly Lazy<IManagedPrincipalConverter<TPrincipal>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPrincipal(typeof(TPrincipal));

        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var permissionIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.PermissionType);

        var securityRoleIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.SecurityRoleType);

        var securityContextTypeIdentityInfo = identityInfoSource.GetIdentityInfo(restrictionBindingInfo.SecurityContextTypeType);

        var innerServiceType = typeof(ManagedPrincipalConverter<,,,,,,,,>).MakeGenericType(
            bindingInfo.PrincipalType,
            bindingInfo.PermissionType,
            bindingInfo.SecurityRoleType,
            restrictionBindingInfo.PermissionRestrictionType,
            restrictionBindingInfo.SecurityContextTypeType,
            restrictionBindingInfo.SecurityContextObjectIdentType,
            permissionIdentityInfo.IdentityType,
            securityRoleIdentityInfo.IdentityType,
            securityContextTypeIdentityInfo.IdentityType
        );

        return (IManagedPrincipalConverter<TPrincipal>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            bindingInfo,
            restrictionBindingInfo,
            permissionIdentityInfo,
            securityRoleIdentityInfo,
            securityContextTypeIdentityInfo);
    });

    public Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.ToManagedPrincipalAsync(principal, cancellationToken);
}

public class ManagedPrincipalConverter<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent,
    TPermissionIdent, TSecurityRoleIdent, TSecurityContextTypeIdent>(
    GeneralPermissionBindingInfo<TPermission, TPrincipal, TSecurityRole> bindingInfo,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission> restrictionBindingInfo,
    IQueryableSource queryableSource,
    IManagedPrincipalHeaderConverter<TPrincipal> headerConverter,
    ISecurityRoleSource securityRoleSource,
    ISecurityContextInfoSource securityContextInfoSource,
    IdentityInfo<TPermission, TPermissionIdent> permissionIdentityInfo,
    IdentityInfo<TSecurityRole, TSecurityRoleIdent> securityRoleIdentityInfo,
    IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> securityContextTypeIdentityInfo) : IManagedPrincipalConverter<TPrincipal>
    where TPrincipal : class
    where TPermission : class
    where TPermissionRestriction : class
    where TPermissionIdent : notnull
    where TSecurityRoleIdent : notnull
    where TSecurityContextTypeIdent : notnull
{
    public async Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken)
    {
        var permissions = await queryableSource.GetQueryable<TPermission>()
            .Where(bindingInfo.Principal.Path.Select(p => p == principal))
            .GenericToListAsync(cancellationToken);

        return new ManagedPrincipal(
            headerConverter.Convert(principal),
            await permissions.SyncWhenAll(permission => this.ToManagedPermissionAsync(permission, cancellationToken)));
    }


    private async Task<ManagedPermission> ToManagedPermissionAsync(TPermission permission, CancellationToken cancellationToken)
    {
        var dbRestrictions = await queryableSource.GetQueryable<TPermissionRestriction>()
            .Where(restrictionBindingInfo.Permission.Path.Select(p => p == permission))
            .GenericToListAsync(cancellationToken);

        var securityRoleId = bindingInfo.SecurityRole.Getter.Composite(securityRoleIdentityInfo.Id.Getter).Invoke(permission);

        var securityRole = securityRoleSource.GetSecurityRole(TypedSecurityIdentity.Create(securityRoleId));

        var purePermission = dbRestrictions.GroupBy(
                restrictionBindingInfo.SecurityContextType.Getter.Composite(securityContextTypeIdentityInfo.Id.Getter),
                restrictionBindingInfo.SecurityContextObjectId.Getter)

            .ToDictionary(g => g.Key, g => g.ToList());

        var convertedPermission = purePermission
            .ChangeKey(securityContextTypeId => securityContextInfoSource.GetSecurityContextInfo(TypedSecurityIdentity.Create(securityContextTypeId)).Type)
            .ChangeValue(Array (idents) => idents.ToArray());

        return new ManagedPermission(
            TypedSecurityIdentity.Create(permissionIdentityInfo.Id.Getter(permission)),
            bindingInfo.IsReadonly,
            securityRole,
            bindingInfo.GetSafePeriod(permission),
            bindingInfo.GetSafeComment(permission),
            convertedPermission);
    }
}