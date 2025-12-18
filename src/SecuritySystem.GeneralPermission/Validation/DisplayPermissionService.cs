using CommonFramework;
using CommonFramework.VisualIdentitySource;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.ExternalSystem.SecurityContextStorage;

namespace SecuritySystem.GeneralPermission.Validation;


public class DisplayPermissionService<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    GeneralPermissionBindingInfo<TPermission, TPrincipal, TSecurityRole> bindingInfo,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> restrictionBindingInfo,
    IDomainObjectDisplayService domainObjectDisplayService,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityContextStorage securityContextStorage,
    VisualIdentityInfo<TSecurityContextType> securityContextTypeVisualIdentityInfo)
    : IDisplayPermissionService<PermissionData<TPermission, TPermissionRestriction>>
    where TSecurityRole : class
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
{
    public string ToString(PermissionData<TPermission, TPermissionRestriction> permissionData)
    {
        return this.GetPermissionVisualParts(permissionData).Join(" | ");
    }

    private IEnumerable<string> GetPermissionVisualParts(PermissionData<TPermission, TPermissionRestriction> permissionData)
    {
        var permission = permissionData.Permission;

        yield return $"Role: {domainObjectDisplayService.ToString(bindingInfo.SecurityRole.Getter(permissionData.Permission))}";

        if (bindingInfo.PermissionPeriod != null)
        {
            yield return $"Period: {bindingInfo.PermissionPeriod.Getter(permission)}";
        }

        foreach (var securityContextTypeGroup in permissionData.Restrictions.GroupBy(
                     restrictionBindingInfo.SecurityContextType.Getter,
                     restrictionBindingInfo.SecurityContextObjectId.Getter))
        {
            var securityContextTypeName = securityContextTypeVisualIdentityInfo.Name.Getter(securityContextTypeGroup.Key);

            var securityContextInfo = securityContextInfoSource.GetSecurityContextInfo(securityContextTypeName);

            var securityEntities = securityContextStorage
                .GetTyped(securityContextInfo.Type)
                .GetSecurityContextsByIdents(securityContextTypeGroup.ToArray());

            yield return $"{securityContextTypeName}: {securityEntities.Select(v => v.Name).Join(", ")}";
        }
    }
}