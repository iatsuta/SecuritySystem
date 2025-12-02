using CommonFramework;

using GenericQueryable;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.GeneralPermission;

public class GeneralPrincipalSourceService<TPrincipal, TPermission>(
	IQueryableSource queryableSource,
	IAvailablePermissionSource<TPermission> availablePermissionSource,
	UserSourceInfo<TPrincipal> userSourceInfo,
	ITypedPrincipalConverter<TPrincipal> typedPrincipalConverter,
	IPermissionToPrincipalInfo<TPrincipal, TPermission> permissionToPrincipalInfo,
	IPrincipalFilterFactory<TPrincipal> principalFilterFactory) : IPrincipalSourceService
	where TPrincipal : class
{
	private readonly IQueryable<TPrincipal> principalQueryable = queryableSource.GetQueryable<TPrincipal>();

	public async Task<IEnumerable<TypedPrincipalHeader>> GetPrincipalsAsync(
		string nameFilter,
		int limit,
		CancellationToken cancellationToken)
	{
		return await principalQueryable
			.Pipe(
				!string.IsNullOrWhiteSpace(nameFilter),
				q => q.Where(userSourceInfo.Name.Path.Select(principalName => principalName.Contains(nameFilter))))
			.Select(typedPrincipalConverter.GetToHeaderExpression())
			.GenericToListAsync(cancellationToken);
	}

	public async Task<TypedPrincipal?> TryGetPrincipalAsync(string principalId, CancellationToken cancellationToken)
	{
		var filter = principalFilterFactory.CreateFilterById(principalId);

		var principal = await principalQueryable.Where(filter).GenericSingleOrDefaultAsync(cancellationToken);

		if (principal is null)
		{
			return null;
		}
		else
		{
			return await typedPrincipalConverter.ToTypedPrincipalAsync(principal, cancellationToken);
		}
	}

	public async Task<IEnumerable<string>> GetLinkedPrincipalsAsync(
		IEnumerable<SecurityRole> securityRoles,
		CancellationToken cancellationToken)
	{
		return await availablePermissionSource
			.GetAvailablePermissionsQueryable(
				DomainSecurityRule.ExpandedRolesSecurityRule.Create(securityRoles) with
				{
					CustomCredential = new SecurityRuleCredential.AnyUserCredential()
				})
			.Select(permissionToPrincipalInfo.ToPrincipal.Path.Select(userSourceInfo.Name.Path))
			.Distinct()
			.GenericToListAsync(cancellationToken);
	}
}