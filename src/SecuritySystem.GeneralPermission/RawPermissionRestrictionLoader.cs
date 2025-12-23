using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.GeneralPermission;

public class RawPermissionRestrictionLoader<TPermission>(
    IServiceProvider serviceProvider,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IRawPermissionRestrictionLoader<TPermission>
{
    private readonly Lazy<IRawPermissionRestrictionLoader<TPermission>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(typeof(TPermission));

        var innerServiceType = typeof(RawPermissionRestrictionLoader<,>).MakeGenericType(
            restrictionBindingInfo.PermissionType,
            restrictionBindingInfo.PermissionRestrictionType);

        return (IRawPermissionRestrictionLoader<TPermission>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType);
    });

    public Task<Dictionary<Type, Array>> LoadAsync(TPermission permission, CancellationToken cancellationToken) =>
        lazyInnerService.Value.LoadAsync(permission, cancellationToken);
}

public class RawPermissionRestrictionLoader<TPermission, TPermissionRestriction>(
    IPermissionRestrictionLoader<TPermission, TPermissionRestriction> permissionRestrictionLoader,
    IPermissionRestrictionRawConverter<TPermissionRestriction> permissionRestrictionRawConverter) : IRawPermissionRestrictionLoader<TPermission>
{
    public async Task<Dictionary<Type, Array>> LoadAsync(TPermission permission, CancellationToken cancellationToken)
    {
        var dbRestrictions = await permissionRestrictionLoader.LoadAsync(permission, cancellationToken);

        return permissionRestrictionRawConverter.Convert(dbRestrictions);
    }
}