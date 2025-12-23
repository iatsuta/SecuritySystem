using Microsoft.Extensions.DependencyInjection;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.GeneralPermission.Validation.Permission;

namespace SecuritySystem.GeneralPermission.Validation.Principal;

public class PrincipalRootValidator<TPrincipal, TPermission, TPermissionRestriction>(
    IEnumerable<IPrincipalValidator<TPrincipal, TPermission, TPermissionRestriction>> validators,
    [FromKeyedServices("Root")] IPermissionValidator<TPermission, TPermissionRestriction> permissionRootValidator)
    : IPrincipalValidator<TPrincipal, TPermission, TPermissionRestriction>
{
    public async Task ValidateAsync(PrincipalData<TPrincipal, TPermission, TPermissionRestriction> principalData, CancellationToken cancellationToken)
    {
        foreach (var validator in validators)
        {
            await validator.ValidateAsync(principalData, cancellationToken);
        }

        foreach (var permission in principalData.PermissionDataList)
        {
            await permissionRootValidator.ValidateAsync(permission, cancellationToken);
        }
    }
}