using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public class PrincipalRootValidator(IServiceProvider serviceProvider) : ISecurityValidator<PrincipalData>
{
    //public PermissionGeneralValidator(
    //    ISecurityValidator<PermissionRestriction> permissionRestrictionValidator,
    //    [FromKeyedServices(PermissionRequiredContextValidator.Key)] ISecurityValidator<TPermission> permissionRequiredContextValidator,
    //    [FromKeyedServices(PermissionDelegateValidator.Key)] ISecurityValidator<TPermission> permissionDelegateValidator)
    //{
    //    this.RuleForEach(permission => permission.Restrictions).SetValidator(permissionRestrictionValidator);

    //    this.Include(permissionRequiredContextValidator);
    //    this.Include(permissionDelegateValidator);
    //}
    public async Task ValidateAsync(PrincipalData principalData, CancellationToken cancellationToken)
    {
        //principalData.PrincipalType
        //throw new NotImplementedException();
    }
}


public class PrincipalRootValidator<TPrincipal, TPermission, TPermissionRestriction> : ISecurityValidator<PrincipalData<TPrincipal, TPermission, TPermissionRestriction>>
{
    //public PermissionGeneralValidator(
    //    ISecurityValidator<PermissionRestriction> permissionRestrictionValidator,
    //    [FromKeyedServices(PermissionRequiredContextValidator.Key)] ISecurityValidator<TPermission> permissionRequiredContextValidator,
    //    [FromKeyedServices(PermissionDelegateValidator.Key)] ISecurityValidator<TPermission> permissionDelegateValidator)
    //{
    //    this.RuleForEach(permission => permission.Restrictions).SetValidator(permissionRestrictionValidator);

    //    this.Include(permissionRequiredContextValidator);
    //    this.Include(permissionDelegateValidator);
    //}
    public async Task ValidateAsync(PrincipalData<TPrincipal, TPermission, TPermissionRestriction> principalData, CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
    }
}