using CommonFramework.DependencyInjection;

namespace SecuritySystem.GeneralPermission;

public class PermissionRestrictionRawConverter<TPermissionRestriction>(
    IServiceProxyFactory serviceProxyFactory,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource)
    : IPermissionRestrictionRawConverter<TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionRawConverter<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var innerServiceType = typeof(PermissionRestrictionRawConverter<,,>).MakeGenericType(
            restrictionBindingInfo.PermissionRestrictionType,
            restrictionBindingInfo.SecurityContextTypeType,
            restrictionBindingInfo.SecurityContextObjectIdentType);

        return serviceProxyFactory.Create<IPermissionRestrictionRawConverter<TPermissionRestriction>>(
            innerServiceType,
            restrictionBindingInfo);
    });

    public Dictionary<Type, Array> Convert(IEnumerable<TPermissionRestriction> permissionRestrictions) =>
        this.lazyInnerService.Value.Convert(permissionRestrictions);
}


public class PermissionRestrictionRawConverter<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(

    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> restrictionBindingInfo,
    IPermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction> permissionRestrictionSecurityContextTypeResolver)
    : IPermissionRestrictionRawConverter<TPermissionRestriction>
{
    public Dictionary<Type, Array> Convert(IEnumerable<TPermissionRestriction> permissionRestrictions)
    {
        return permissionRestrictions.GroupBy(permissionRestrictionSecurityContextTypeResolver.Resolve, restrictionBindingInfo.SecurityContextObjectId.Getter)
            .ToDictionary(g => g.Key, Array (g) => g.ToArray());
    }
}