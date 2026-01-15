using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.VisualIdentitySource;

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
	IManagedPrincipalHeaderConverter<TPrincipal> principalHeaderConverter,
    IManagedPrincipalConverter<TPrincipal> principalConverter,
    IUserQueryableSource<TPrincipal> userQueryableSource) : IPrincipalSourceService
	where TPrincipal : class
{
	private readonly PropertyAccessors<TPrincipal, string> nameAccessors = visualIdentityInfoSource.GetVisualIdentityInfo<TPrincipal>().Name;

	private readonly IQueryable<TPrincipal> principalQueryable = queryableSource.GetQueryable<TPrincipal>();

    public Type PrincipalType { get; } = typeof(TPrincipal);

    public async Task<IEnumerable<ManagedPrincipalHeader>> GetPrincipalsAsync(string nameFilter, int limit, CancellationToken cancellationToken)
	{
		return await principalQueryable
			.Pipe(
				!string.IsNullOrWhiteSpace(nameFilter),
				q => q.Where(this.nameAccessors.Path.Select(principalName => principalName.Contains(nameFilter))))
			.Select(principalHeaderConverter.ConvertExpression)
			.GenericToListAsync(cancellationToken);
	}

	public async Task<ManagedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken)
	{
		var principal = await userQueryableSource.GetQueryable(userCredential).GenericSingleOrDefaultAsync(cancellationToken);

		if (principal is null)
		{
			return null;
		}
		else
		{
			return await principalConverter.ToManagedPrincipalAsync(principal, cancellationToken);
		}
	}

	public async Task<IEnumerable<string>> GetLinkedPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, CancellationToken cancellationToken)
	{
		return await availablePrincipalSource.GetAvailablePrincipalsQueryable(
				DomainSecurityRule.ExpandedRolesSecurityRule.Create(securityRoles) with
				{
					CustomCredential = new SecurityRuleCredential.AnyUserCredential()
				})
			.Select(this.nameAccessors.Path)
            .Distinct()
            .GenericToListAsync(cancellationToken);
	}
}