using System.Linq.Expressions;

using CommonFramework;

using GenericQueryable;

using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.GeneralPermission;

public class GeneralPrincipalSourceService<TPrincipal>(
	IQueryableSource queryableSource,
	IVisualIdentityInfoSource visualIdentityInfoSource,
	IAvailablePrincipalSource<TPrincipal> availablePrincipalSource,
	ITypedPrincipalConverter<TPrincipal> typedPrincipalConverter,
	IUserQueryableSource<TPrincipal> userQueryableSource) : IPrincipalSourceService
	where TPrincipal : class
{
	private readonly Expression<Func<TPrincipal, string>> namePath = visualIdentityInfoSource.GetVisualIdentityInfo<TPrincipal>().Name.Path;

	private readonly IQueryable<TPrincipal> principalQueryable = queryableSource.GetQueryable<TPrincipal>();

	public async Task<IEnumerable<TypedPrincipalHeader>> GetPrincipalsAsync(string nameFilter, int limit, CancellationToken cancellationToken)
	{
		return await principalQueryable
			.Pipe(
				!string.IsNullOrWhiteSpace(nameFilter),
				q => q.Where(namePath.Select(principalName => principalName.Contains(nameFilter))))
			.Select(typedPrincipalConverter.GetToHeaderExpression())
			.GenericToListAsync(cancellationToken);
	}

	public async Task<TypedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken)
	{
		var principal = await userQueryableSource.GetQueryable(userCredential).GenericSingleOrDefaultAsync(cancellationToken);

		if (principal is null)
		{
			return null;
		}
		else
		{
			return await typedPrincipalConverter.ToTypedPrincipalAsync(principal, cancellationToken);
		}
	}

	public async Task<IEnumerable<string>> GetLinkedPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, CancellationToken cancellationToken)
	{
		return await availablePrincipalSource.GetAvailablePrincipalsQueryable(
				DomainSecurityRule.ExpandedRolesSecurityRule.Create(securityRoles) with
				{
					CustomCredential = new SecurityRuleCredential.AnyUserCredential()
				})
			.Select(namePath)
			.GenericToListAsync(cancellationToken);
	}
}