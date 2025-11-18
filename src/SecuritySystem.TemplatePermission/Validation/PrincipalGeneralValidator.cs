using FluentValidation;

using Framework.Authorization.Domain;

namespace SecuritySystem.TemplatePermission.Validation;

public class PrincipalGeneralValidator : AbstractValidator<Principal>, IPrincipalGeneralValidator
{
    public PrincipalGeneralValidator(
        IPrincipalUniquePermissionValidator uniquePermissionValidator,
        IPermissionGeneralValidator permissionGeneralValidator)
    {
        this.Include(uniquePermissionValidator);

        this.RuleForEach(principal => principal.Permissions).SetValidator(permissionGeneralValidator);
    }
}
