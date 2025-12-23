using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Validation;

namespace SecuritySystem.GeneralPermission.Validation.PermissionRestriction;

public class AllowedTypePermissionRestrictionValidator<TPermissionRestriction>(
    IServiceProvider serviceProvider,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IPermissionRestrictionValidator<TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionValidator<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var innerServiceType = typeof(AllowedTypePermissionRestrictionValidator<,,,>)
            .MakeGenericType(
                restrictionBindingInfo.PermissionRestrictionType,
                restrictionBindingInfo.SecurityContextTypeType,
                restrictionBindingInfo.SecurityContextObjectIdentType,
                restrictionBindingInfo.PermissionType);

        return (IPermissionRestrictionValidator<TPermissionRestriction>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            restrictionBindingInfo);
    });

    public Task ValidateAsync(TPermissionRestriction value, CancellationToken cancellationToken) =>
        lazyInnerService.Value.ValidateAsync(value, cancellationToken);
}

public class AllowedTypePermissionRestrictionValidator<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission>(
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission> restrictionBindingInfo,
    IPermissionSecurityRoleResolver<TPermission> permissionSecurityRoleResolver,
    IPermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction> permissionRestrictionSecurityContextTypeResolver)
    : IPermissionRestrictionValidator<TPermissionRestriction>
    where TSecurityContextObjectIdent : notnull
{
    public async Task ValidateAsync(TPermissionRestriction permissionRestriction, CancellationToken cancellationToken)
    {
        var permission = restrictionBindingInfo.Permission.Getter(permissionRestriction);

        var securityContextType = permissionRestrictionSecurityContextTypeResolver.Resolve(permissionRestriction);

        var securityRole = permissionSecurityRoleResolver.Resolve(permission);

        var allowedSecurityContexts = securityRole.Information.Restriction.SecurityContextTypes;

        var allowed = allowedSecurityContexts == null || allowedSecurityContexts.Contains(securityContextType);

        if (!allowed)
        {
            throw new SecuritySystemValidationException($"Invalid SecurityContextType: {securityContextType.Name}");
        }
    }
}