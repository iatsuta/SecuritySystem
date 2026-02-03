using CommonFramework;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Validation;

namespace SecuritySystem.GeneralPermission.Validation.Permission;

public class PermissionRequiredContextValidator<TPermission, TPermissionRestriction>(
    IPermissionSecurityRoleResolver<TPermission> permissionSecurityRoleResolver,
    ISecurityContextInfoSource securityContextInfoSource,
    IPermissionRestrictionSecurityContextTypeResolver<TPermissionRestriction> permissionRestrictionSecurityContextTypeResolver)
    : IPermissionValidator<TPermission, TPermissionRestriction>
{
    public async Task ValidateAsync(PermissionData<TPermission, TPermissionRestriction> permissionData, CancellationToken cancellationToken)
    {
        var securityRole = permissionSecurityRoleResolver.Resolve(permissionData.Permission);

        if (securityRole.Information.Restriction.SecurityContextRestrictions is { } restrictions)
        {
            var usedTypes = permissionData.Restrictions.Select(permissionRestrictionSecurityContextTypeResolver.Resolve).Distinct().ToList();

            var requiredSecurityContextTypes = restrictions.Where(pair => pair.Required).Select(pair => pair.SecurityContextType);

            var missedTypeInfoList = requiredSecurityContextTypes
                .Except(usedTypes)
                .Select(securityContextInfoSource.GetSecurityContextInfo)
                .Select(bindingInfo => bindingInfo.Name)
                .ToList();

            if (missedTypeInfoList.Any())
            {
                var missedTypes = missedTypeInfoList.Join(", ");

                throw new SecuritySystemValidationException($"{typeof(TPermission).Name} must contain the required contexts: {missedTypes}");
            }
        }
    }
}