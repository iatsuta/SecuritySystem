using CommonFramework;
using CommonFramework.IdentitySource;

using SecuritySystem.Services;
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
    IIdentityInfoSource identityInfoSource,
    IDomainObjectIdentsParser<TSecurityContextObjectIdent> domainObjectIdentsParser) : IPermissionRestrictionValidator<TPermissionRestriction>
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

                if (!await this.IsAllowed(securityContextId, restrictionFilterInfo, cancellationToken))
                {
                    throw new SecuritySystemValidationException($"SecurityContext: '{securityContextId}' denied by filter");
                }
            }
        }
    }

    private Task<bool> IsAllowed(TSecurityContextObjectIdent securityContextId, SecurityContextRestrictionFilterInfo restrictionFilterInfo, CancellationToken cancellationToken)
    {
        var identityInfo = identityInfoSource.GetIdentityInfo(restrictionFilterInfo.SecurityContextType);

        return new Func<TSecurityContextObjectIdent, SecurityContextRestrictionFilterInfo<ISecurityContext>, IdentityInfo<ISecurityContext, Ignore>, CancellationToken, Task<bool>>
                (this.IsAllowed<ISecurityContext, Ignore>)
            .CreateGenericMethod(restrictionFilterInfo.SecurityContextType, identityInfo.IdentityType)
            .Invoke<Task<bool>>(this, securityContextId, restrictionFilterInfo, identityInfo, cancellationToken);
    }

    private async Task<bool> IsAllowed<TSecurityContext, TSecurityContextIdent>(
        TSecurityContextObjectIdent securityContextId,
        SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo,
        IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo, CancellationToken cancellationToken)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull
    {
        var convertedIdent = domainObjectIdentsParser.Parse(typeof(TSecurityContext), [securityContextId]).Cast<TSecurityContextIdent>().Single();

        return securityContextSource.GetQueryable(restrictionFilterInfo)
            .Select(identityInfo.Id.Path)
            .Contains(convertedIdent);
    }
}