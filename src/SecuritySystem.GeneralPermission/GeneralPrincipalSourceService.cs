using System.Linq.Expressions;

using CommonFramework;

using GenericQueryable;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.GeneralPermission;

public class GeneralPrincipalSourceService<TPrincipal, TPermission>(
    IQueryableSource queryableSource,
    ISecurityRoleSource securityRoleSource,
    ISecurityContextInfoSource securityContextInfoSource,
    IAvailablePermissionSource<TPermission> availablePermissionSource,
    UserSourceInfo<TPrincipal> userSourceInfo
    ) : IPrincipalSourceService
	where TPrincipal : class
{
	private readonly IQueryable<TPrincipal> principalQueryable = queryableSource.GetQueryable<TPrincipal>();

	private readonly Expression<Func<TPrincipal, TypedPrincipalHeader>> toTypedPrincipalHeaderExpression; //principal => new TypedPrincipalHeader(principal.Id, principal.Name, false)

	public async Task<IEnumerable<TypedPrincipalHeader>> GetPrincipalsAsync(
        string nameFilter,
        int limit,
        CancellationToken cancellationToken)
	{
		return await principalQueryable
			.Pipe(
				!string.IsNullOrWhiteSpace(nameFilter),
				q => q.Where(userSourceInfo.NamePath.Select(principalName => principalName.Contains(nameFilter))))
					.Select(this.toTypedPrincipalHeaderExpression)
					.GenericToListAsync(cancellationToken);
	}

    private async Task<TypedPrincipal?> TryGetPrincipalAsync(Expression<Func<TPrincipal, bool>> filter, CancellationToken cancellationToken)
    {
        var principal = await principalQueryable
                                                 .Where(filter)
                                                 .WithFetch(r => r.Fetch(v => v.Permissions).ThenFetch(v => v.Restrictions))
                                                 .GenericSingleOrDefaultAsync(cancellationToken);

        if (principal is null)
        {
            return null;
        }
        else
        {
            return new TypedPrincipal(
                new TypedPrincipalHeader(principal.Id, principal.Name, false),
                principal.Permissions
                         .Select(
                             permission => new TypedPermission(
                                 permission.Id,
                                 false,
                                 securityRoleSource.GetSecurityRole(permission.Role.Id),
                                 permission.Period.StartDate,
                                 permission.Period.EndDate,
                                 permission.Comment,
                                 permission.Restrictions
                                           .GroupBy(r => r.SecurityContextType.Id, r => r.SecurityContextId)
                                           .ToDictionary(
                                               g => securityContextInfoSource.GetSecurityContextInfo(g.Key).Type,
                                               Array (g) => g.ToArray())))
                         .ToList());
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
                     .Select(permission => permission.TPrincipal.Name)
                     .Distinct()
                     .GenericToListAsync(cancellationToken);
    }
}
