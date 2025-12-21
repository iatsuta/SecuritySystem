using CommonFramework;
using CommonFramework.GenericRepository;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class ManagedPrincipalConverter<TPrincipal>(
    IServiceProvider serviceProvider,
    IPermissionBindingInfoSource bindingInfoSource,
    IGeneralPermissionBindingInfoSource generalBindingInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IManagedPrincipalConverter<TPrincipal>
{
    private readonly Lazy<IManagedPrincipalConverter<TPrincipal>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPrincipal(typeof(TPrincipal));

        var generalBindingInfo = generalBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var innerServiceType = typeof(ManagedPrincipalConverter<,,,,,>).MakeGenericType(
            bindingInfo.PrincipalType,
            bindingInfo.PermissionType,
            generalBindingInfo.SecurityRoleType,
            restrictionBindingInfo.PermissionRestrictionType,
            restrictionBindingInfo.SecurityContextTypeType,
            restrictionBindingInfo.SecurityContextObjectIdentType
        );

        return (IManagedPrincipalConverter<TPrincipal>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            bindingInfo,
            restrictionBindingInfo);
    });

    public Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.ToManagedPrincipalAsync(principal, cancellationToken);
}

public class ManagedPrincipalConverter<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    GeneralPermissionBindingInfo<TPermission, TSecurityRole> generalBindingInfo,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission> restrictionBindingInfo,
    IQueryableSource queryableSource,
    IManagedPrincipalHeaderConverter<TPrincipal> headerConverter,
    ISecurityRoleSource securityRoleSource,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityIdentityExtractorFactory securityIdentityExtractorFactory) : IManagedPrincipalConverter<TPrincipal>
    where TPrincipal : class
    where TPermission : class
    where TPermissionRestriction : class
    where TSecurityContextType : class
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

        var dbSecurityRole = generalBindingInfo.SecurityRole.Getter(permission);

        var securityRole = securityRoleSource.GetSecurityRole(securityIdentityExtractorFactory.Create<TSecurityRole>().Extract(dbSecurityRole));

        var purePermission = dbRestrictions.GroupBy(
                restrictionBindingInfo.SecurityContextType.Getter,
                restrictionBindingInfo.SecurityContextObjectId.Getter)

            .ToDictionary(g => g.Key, g => g.ToList());

        var convertedPermission = purePermission
            .ChangeKey(securityContextType => securityContextInfoSource
                .GetSecurityContextInfo(securityIdentityExtractorFactory.Create<TSecurityContextType>().Extract(securityContextType)).Type)
            .ChangeValue(Array (idents) => idents.ToArray());

        return new ManagedPermission(
            securityIdentityExtractorFactory.Create<TPermission>().Extract(permission),
            bindingInfo.IsReadonly,
            securityRole,
            bindingInfo.GetSafePeriod(permission),
            bindingInfo.GetSafeComment(permission),
            convertedPermission);
    }
}