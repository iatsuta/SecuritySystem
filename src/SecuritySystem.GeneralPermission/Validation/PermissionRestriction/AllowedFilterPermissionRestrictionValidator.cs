using CommonFramework;
using CommonFramework.DependencyInjection;
using CommonFramework.IdentitySource;
using SecuritySystem.Validation;

namespace SecuritySystem.GeneralPermission.Validation.PermissionRestriction;

public class AllowedFilterPermissionRestrictionValidator<TPermissionRestriction>(
    IServiceProxyFactory serviceProxyFactory,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IPermissionRestrictionValidator<TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionValidator<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var innerServiceType = typeof(AllowedFilterPermissionRestrictionValidator<,,,>)
            .MakeGenericType(
                restrictionBindingInfo.PermissionRestrictionType,
                restrictionBindingInfo.SecurityContextTypeType,
                restrictionBindingInfo.SecurityContextObjectIdentType,
                restrictionBindingInfo.PermissionType);

        return serviceProxyFactory.Create<IPermissionRestrictionValidator<TPermissionRestriction>>(
            innerServiceType,
            restrictionBindingInfo);
    });

    public Task ValidateAsync(TPermissionRestriction value, CancellationToken cancellationToken) =>
        lazyInnerService.Value.ValidateAsync(value, cancellationToken);
}

public class AllowedFilterPermissionRestrictionValidator<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission>(
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission> restrictionBindingInfo,
    IPermissionSecurityRoleResolver<TPermission> permissionSecurityRoleResolver,
    ISecurityContextSource securityContextSource,
    IPermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction> permissionRestrictionSecurityContextTypeResolver,
    IIdentityInfoSource identityInfoSource) : IPermissionRestrictionValidator<TPermissionRestriction>
    where TSecurityContextObjectIdent : notnull
{
    public async Task ValidateAsync(TPermissionRestriction permissionRestriction, CancellationToken cancellationToken)
    {
        var permission = restrictionBindingInfo.Permission.Getter(permissionRestriction);

        var securityRole = permissionSecurityRoleResolver.Resolve(permission);

        if (securityRole.Information.Restriction.SecurityContextRestrictions is { } securityContextRestrictions)
        {
            var securityContextType = permissionRestrictionSecurityContextTypeResolver.Resolve(permissionRestriction);

            if (securityContextRestrictions.SingleOrDefault(scr => scr.SecurityContextType == securityContextType)?.RawFilter is { } restrictionFilterInfo)
            {
                var securityContextId = restrictionBindingInfo.SecurityContextObjectId.Getter(permissionRestriction);

                if (!this.IsAllowed(securityContextId, restrictionFilterInfo))
                {
                    throw new SecuritySystemValidationException($"SecurityContext: '{securityContextId}' denied by filter.");
                }
            }
        }
    }

    private bool IsAllowed(TSecurityContextObjectIdent securityContextId, SecurityContextRestrictionFilterInfo restrictionFilterInfo)
    {
        return new Func<TSecurityContextObjectIdent, SecurityContextRestrictionFilterInfo<ISecurityContext>, bool>(this.IsAllowed)
            .CreateGenericMethod(restrictionFilterInfo.SecurityContextType)
            .Invoke<bool>(this, securityContextId, restrictionFilterInfo);
    }

    private bool IsAllowed<TSecurityContext>(
        TSecurityContextObjectIdent securityContextId,
        SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextObjectIdent>();

        return securityContextSource.GetQueryable(restrictionFilterInfo).Select(identityInfo.Id.Path).Contains(securityContextId);
    }
}