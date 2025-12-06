namespace SecuritySystem.GeneralPermission;

public class GeneralAvailableSecurityRoleSource<TPermission>(
	IAvailablePermissionSource<TPermission> availablePermissionSource,
	ISecurityRoleSource securityRoleSource,
	SecurityRuleCredential securityRuleCredential)
{
	public async Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken)
	{
		var dbRequest =

			from permission in availablePermissionSource.GetAvailablePermissionsQueryable(DomainSecurityRule.AnyRole with { CustomCredential = securityRuleCredential })

			select permission.Role.Id;

		var dbRolesIdents = await dbRequest.Distinct().GenericToListAsync(cancellationToken);

		return dbRolesIdents.Select(securityRoleSource.GetSecurityRole);
	}
}