using CommonFramework.DependencyInjection;
using SecuritySystem.ExternalSystem.SecurityContextStorage;
using SecuritySystem.Validation;

namespace SecuritySystem.GeneralPermission.Validation.PermissionRestriction;

public class ExistsPermissionRestrictionValidator<TPermissionRestriction>(
    IServiceProxyFactory serviceProxyFactory,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IPermissionRestrictionValidator<TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionValidator<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var innerServiceType = typeof(ExistsPermissionRestrictionValidator<,,>)
            .MakeGenericType(
                restrictionBindingInfo.PermissionRestrictionType,
                restrictionBindingInfo.SecurityContextTypeType,
                restrictionBindingInfo.SecurityContextObjectIdentType);

        return serviceProxyFactory.Create<IPermissionRestrictionValidator<TPermissionRestriction>>(
            innerServiceType,
            restrictionBindingInfo);
    });

    public Task ValidateAsync(TPermissionRestriction value, CancellationToken cancellationToken) =>
        lazyInnerService.Value.ValidateAsync(value, cancellationToken);
}

public class ExistsPermissionRestrictionValidator<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> restrictionBindingInfo,
    ISecurityContextStorage securityContextStorage,
    IPermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction> permissionRestrictionSecurityContextTypeResolver)
    : IPermissionRestrictionValidator<TPermissionRestriction>
    where TSecurityContextObjectIdent : notnull
{
    public async Task ValidateAsync(TPermissionRestriction permissionRestriction, CancellationToken cancellationToken)
    {
        var securityContextObjectId = restrictionBindingInfo.SecurityContextObjectId.Getter(permissionRestriction);

        var securityContextType = permissionRestrictionSecurityContextTypeResolver.Resolve(permissionRestriction);

        var typedSecurityContextStorage = securityContextStorage.GetTyped(securityContextType);

        if (!typedSecurityContextStorage.IsExists(TypedSecurityIdentity.Create(securityContextObjectId)))
        {
            throw new SecuritySystemValidationException($"{securityContextType.Name} with id '{securityContextObjectId}' not exists.");
        }
    }
}