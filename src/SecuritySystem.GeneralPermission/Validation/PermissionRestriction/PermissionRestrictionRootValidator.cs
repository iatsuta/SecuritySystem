namespace SecuritySystem.GeneralPermission.Validation.PermissionRestriction;

public class PermissionRestrictionRootValidator<TPermissionRestriction>(
    IEnumerable<IPermissionRestrictionValidator<TPermissionRestriction>> validators) : IPermissionRestrictionValidator<TPermissionRestriction>
{
    public async Task ValidateAsync(TPermissionRestriction permissionRestriction, CancellationToken cancellationToken)
    {
        foreach (var validator in validators)
        {
            await validator.ValidateAsync(permissionRestriction, cancellationToken);
        }
    }
}