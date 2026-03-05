using CommonFramework;
using CommonFramework.GenericRepository;

using GenericQueryable;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public class PermissionRestrictionLoader<TPermission, TPermissionRestriction>(
    IServiceProxyFactory serviceProxyFactory,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource)
    : IPermissionRestrictionLoader<TPermission, TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionLoader<TPermission, TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var bindingInfo = restrictionBindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var innerServiceType = typeof(PermissionRestrictionLoader<,,,>).MakeGenericType(
            bindingInfo.PermissionType,
            bindingInfo.PermissionRestrictionType,
            bindingInfo.SecurityContextTypeType,
            bindingInfo.SecurityContextObjectIdentType);

        return serviceProxyFactory.Create<IPermissionRestrictionLoader<TPermission, TPermissionRestriction>>(
            innerServiceType,
            bindingInfo);
    });

    public IAsyncEnumerable<TPermissionRestriction> LoadAsync(TPermission permission) => this.lazyInnerService.Value.LoadAsync(permission);

    public ValueTask<PermissionData<TPermission, TPermissionRestriction>> ToPermissionData(TPermission dbPermission, CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.ToPermissionData(dbPermission, cancellationToken);
}

public class PermissionRestrictionLoader<TPermission, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    IQueryableSource queryableSource,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission> restrictionBindingInfo)
    : IPermissionRestrictionLoader<TPermission, TPermissionRestriction>
    where TPermission : class
    where TPermissionRestriction : class
{
    public IAsyncEnumerable<TPermissionRestriction> LoadAsync(TPermission permission) =>

        queryableSource.GetQueryable<TPermissionRestriction>()
            .Where(restrictionBindingInfo.Permission.Path.Select(p => p == permission))
            .GenericAsAsyncEnumerable();
}