using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public class PrincipalGeneralValidator<TPrincipal> : ISecurityValidator<TPrincipal>
{
    public const string Key = "General";

    //public PermissionGeneralValidator(
    //    ISecurityValidator<PermissionRestriction> permissionRestrictionValidator,
    //    [FromKeyedServices(PermissionRequiredContextValidator.Key)] ISecurityValidator<TPermission> permissionRequiredContextValidator,
    //    [FromKeyedServices(PermissionDelegateValidator.Key)] ISecurityValidator<TPermission> permissionDelegateValidator)
    //{
    //    this.RuleForEach(permission => permission.Restrictions).SetValidator(permissionRestrictionValidator);

    //    this.Include(permissionRequiredContextValidator);
    //    this.Include(permissionDelegateValidator);
    //}
    public async Task ValidateAsync(TPrincipal principal, CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
    }
}
