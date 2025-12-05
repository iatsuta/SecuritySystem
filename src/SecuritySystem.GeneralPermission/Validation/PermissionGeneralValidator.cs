using Microsoft.Extensions.DependencyInjection;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public class PermissionGeneralValidator : AbstractValidator<TPermission>, IPermissionGeneralValidator
{
    public PermissionGeneralValidator(
        IValidator<PermissionRestriction> permissionRestrictionValidator,
        [FromKeyedServices(PermissionRequiredContextValidator.Key)] IValidator<TPermission> permissionRequiredContextValidator,
        [FromKeyedServices(PermissionDelegateValidator.Key)] IValidator<TPermission> permissionDelegateValidator)
    {
        this.RuleForEach(permission => permission.Restrictions).SetValidator(permissionRestrictionValidator);

        this.Include(permissionRequiredContextValidator);
        this.Include(permissionDelegateValidator);
    }
}
