using CommonFramework;

using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class PermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction>(
    IServiceProxyFactory serviceProxyFactory,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IPermissionRestrictionSecurityContextTypeResolver<
    TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var innerServiceType = typeof(PermissionRestrictionSecurityContextTypeResolver<,>).MakeGenericType(
            restrictionBindingInfo.PermissionRestrictionType,
            restrictionBindingInfo.SecurityContextTypeType);

        return serviceProxyFactory.Create<IPermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction>>(
            innerServiceType,
            restrictionBindingInfo);
    });

    public Type Resolve(TPermissionRestriction permissionRestriction) => this.lazyInnerService.Value.Resolve(permissionRestriction);
}

public class PermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction, TSecurityContextType>(
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType> restrictionBindingInfo,
    ISecurityIdentityExtractor<TSecurityContextType> securityContextTypeSecurityIdentityExtractor,
    ISecurityContextInfoSource securityContextInfoSource) : IPermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction>
{
    public Type Resolve(TPermissionRestriction permissionRestriction)
    {
        return restrictionBindingInfo
            .SecurityContextType.Getter(permissionRestriction)
            .Pipe(securityContextTypeSecurityIdentityExtractor.Extract)
            .Pipe(identity => securityContextInfoSource.GetSecurityContextInfo(identity).Type);
    }
}