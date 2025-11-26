namespace SecuritySystem.TemplatePermission.Validation;

public class PrincipalGeneralValidator : AbstractValidator<TPrincipal>, IPrincipalGeneralValidator
{
    public PrincipalGeneralValidator(
        IPrincipalUniquePermissionValidator uniquePermissionValidator,
        IPermissionGeneralValidator permissionGeneralValidator)
    {
        this.Include(uniquePermissionValidator);

        this.RuleForEach(principal => principal.Permissions).SetValidator(permissionGeneralValidator);
    }
}
