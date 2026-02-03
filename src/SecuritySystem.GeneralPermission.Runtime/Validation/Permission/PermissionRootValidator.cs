using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.GeneralPermission.Validation.PermissionRestriction;

namespace SecuritySystem.GeneralPermission.Validation.Permission;

public class PermissionRootValidator<TPermission, TPermissionRestriction>(
    IEnumerable<IPermissionValidator<TPermission, TPermissionRestriction>> validators,
    [FromKeyedServices("Root")] IPermissionRestrictionValidator<TPermissionRestriction> permissionRestrictionRootValidator)
    : IPermissionValidator<TPermission, TPermissionRestriction>
{
    public async Task ValidateAsync(PermissionData<TPermission, TPermissionRestriction> permissionData, CancellationToken cancellationToken)
    {
        foreach (var validator in validators)
        {
            await validator.ValidateAsync(permissionData, cancellationToken);
        }

        foreach (var restriction in permissionData.Restrictions)
        {
            await permissionRestrictionRootValidator.ValidateAsync(restriction, cancellationToken);
        }
    }
}