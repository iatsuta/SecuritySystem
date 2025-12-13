using CommonFramework;
using CommonFramework.IdentitySource;

using GenericQueryable;

namespace SecuritySystem.GeneralPermission.AvailableSecurityRoleSource;

public class GeneralAvailableSecurityRoleSource<TPermission, TSecurityRole, TSecurityRoleIdent>(
	IPermissionToSecurityRoleInfo<TPermission, TSecurityRole> permissionToSecurityRoleInfo,
	IAvailablePermissionSource<TPermission> availablePermissionSource,
	ISecurityRoleSource securityRoleSource,
	IdentityInfo<TSecurityRole, TSecurityRoleIdent> securityRoleIdentity,
	SecurityRuleCredential securityRuleCredential)
	where TSecurityRoleIdent : notnull
{
	public async Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken)
	{
		var dbRolesIdents = await availablePermissionSource
			.GetQueryable(DomainSecurityRule.AnyRole with { CustomCredential = securityRuleCredential })
			.Select(permissionToSecurityRoleInfo.SecurityRole.Path.Select(securityRoleIdentity.Id.Path))
			.Distinct()
			.GenericToListAsync(cancellationToken);

		return dbRolesIdents.Select(ident => securityRoleSource.GetSecurityRole(new SecurityIdentity<TSecurityRoleIdent>(ident)));
	}
}