using CommonFramework;
using CommonFramework.VisualIdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.ExternalSystem.SecurityContextStorage;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public class DisplayPermissionService<TPermission, TPermissionRestriction>(
    IServiceProvider serviceProvider,
    IPermissionBindingInfoSource bindingInfoSource,
    IGeneralPermissionBindingInfoSource generalBindingInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IDisplayPermissionService<TPermission, TPermissionRestriction>
{
    private readonly Lazy<IDisplayPermissionService<TPermission, TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var generalBindingInfo = generalBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var innerServiceType = typeof(DisplayPermissionService<,,>).MakeGenericType(
            generalBindingInfo.PermissionType,
            generalBindingInfo.SecurityRoleType,
            restrictionBindingInfo.PermissionRestrictionType);

        return (IDisplayPermissionService<TPermission, TPermissionRestriction>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            bindingInfo,
            generalBindingInfo);
    });


    public string ToString(PermissionData<TPermission, TPermissionRestriction> permissionData) => this.lazyInnerService.Value.ToString(permissionData);
}


public class DisplayPermissionService<TPermission, TSecurityRole, TPermissionRestriction>(
    PermissionBindingInfo<TPermission> bindingInfo,
    GeneralPermissionBindingInfo<TPermission, TSecurityRole> generalBindingInfo,
    IDomainObjectDisplayService domainObjectDisplayService,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityContextStorage securityContextStorage,
    IPermissionRestrictionRawConverter<TPermissionRestriction> rawPermissionConverter)
    : IDisplayPermissionService<TPermission, TPermissionRestriction>
    where TSecurityRole : class
{
    public string ToString(PermissionData<TPermission, TPermissionRestriction> permissionData)
    {
        return this.GetPermissionVisualParts(permissionData).Join(" | ");
    }

    private IEnumerable<string> GetPermissionVisualParts(PermissionData<TPermission, TPermissionRestriction> permissionData)
    {
        var permission = permissionData.Permission;

        yield return $"Role: {domainObjectDisplayService.ToString(generalBindingInfo.SecurityRole.Getter(permissionData.Permission))}";

        if (bindingInfo.PermissionStartDate != null)
        {
            yield return $"StartDate: {bindingInfo.PermissionStartDate.Getter(permission)}";
        }

        if (bindingInfo.PermissionEndDate != null)
        {
            yield return $"EndDate: {bindingInfo.PermissionEndDate.Getter(permission)}";
        }

        foreach (var securityContextTypeGroup in rawPermissionConverter.Convert(permissionData.Restrictions))
        {
            var securityContextInfo = securityContextInfoSource.GetSecurityContextInfo(securityContextTypeGroup.Key);

            var securityEntities = securityContextStorage
                .GetTyped(securityContextInfo.Type)
                .GetSecurityContextsByIdents(securityContextTypeGroup.Value);

            yield return $"{securityContextInfo.Name}: {securityEntities.Select(v => v.Name).Join(", ")}";
        }
    }
}