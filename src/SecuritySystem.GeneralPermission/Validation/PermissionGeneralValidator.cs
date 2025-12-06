using Microsoft.Extensions.DependencyInjection;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public class PermissionGeneralValidator : AbstractValidator<TPermission>, IPermissionGeneralValidator
{
    public PermissionGeneralValidator(
        ISecurityValidator<PermissionRestriction> permissionRestrictionValidator,
        [FromKeyedServices(PermissionRequiredContextValidator.Key)] ISecurityValidator<TPermission> permissionRequiredContextValidator,
        [FromKeyedServices(PermissionDelegateValidator.Key)] ISecurityValidator<TPermission> permissionDelegateValidator)
    {
        this.RuleForEach(permission => permission.Restrictions).SetValidator(permissionRestrictionValidator);

        this.Include(permissionRequiredContextValidator);
        this.Include(permissionDelegateValidator);
    }
}
