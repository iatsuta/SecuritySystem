//using SecuritySystem.Services;

//namespace SecuritySystem.GeneralPermission.Validation;

//public class PrincipalGeneralValidator<TPrincipal, TPermission> : ISecurityValidator<TPrincipal>
//{
//	public PrincipalGeneralValidator(
//		ISecurityValidator<TPermission> uniquePermissionValidator,
//		IPermissionGeneralValidator permissionGeneralValidator)
//	{
//		this.Include(uniquePermissionValidator);

//		this.RuleForEach(principal => principal.Permissions).SetValidator(permissionGeneralValidator);
//	}
//}