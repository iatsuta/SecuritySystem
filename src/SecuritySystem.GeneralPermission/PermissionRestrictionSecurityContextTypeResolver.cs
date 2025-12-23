using CommonFramework;

using SecuritySystem.Services;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.GeneralPermission;

public class PermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction>(
    IServiceProvider serviceProvider,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IPermissionRestrictionSecurityContextTypeResolver<
    TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var innerServiceType = typeof(PermissionRestrictionSecurityContextTypeResolver<,>).MakeGenericType(
            restrictionBindingInfo.PermissionRestrictionType,
            restrictionBindingInfo.SecurityContextTypeType);

        return (IPermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            restrictionBindingInfo);
    });

    public Type Resolve(TPermissionRestriction permissionRestriction) => this.lazyInnerService.Value.Resolve(permissionRestriction);
}

public class PermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction, TSecurityContextType>(
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType> restrictionBindingInfo,
    ISecurityIdentityExtractorFactory securityIdentityExtractorFactory,
    ISecurityContextInfoSource securityContextInfoSource) : IPermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction>
{
    public Type Resolve(TPermissionRestriction permissionRestriction)
    {
        return restrictionBindingInfo
            .SecurityContextType.Getter(permissionRestriction)
            .Pipe(securityIdentityExtractorFactory.Create<TSecurityContextType>().Extract)
            .Pipe(identity => securityContextInfoSource.GetSecurityContextInfo(identity).Type);
    }
}