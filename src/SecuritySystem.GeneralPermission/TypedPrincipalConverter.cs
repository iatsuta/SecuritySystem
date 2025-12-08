using System.Linq.Expressions;
using CommonFramework.IdentitySource;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public class TypedPrincipalConverter<TPrincipal, TPrincipalIdent>(IdentityInfo<TPrincipal, TPrincipalIdent> identityInfo) : ITypedPrincipalConverter<TPrincipal>
	where TPrincipalIdent : notnull
{
	public Expression<Func<TPrincipal, TypedPrincipalHeader>> GetToHeaderExpression()
	{
		throw new NotImplementedException();
	}

	public Task<TypedPrincipal> ToTypedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	//public async Task<TypedPrincipal> ToTypedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken)
	//{
	//	return new TypedPrincipal(
	//		new TypedPrincipalHeader(principal.Id, principal.Name, false),
	//		principal.Permissions
	//			.Select(
	//				permission => new TypedPermission(
	//					permission.Id,
	//					false,
	//					securityRoleSource.GetSecurityRole(permission.Role.Id),
	//					permission.Period.StartDate,
	//					permission.Period.EndDate,
	//					permission.Comment,
	//					permission.Restrictions
	//						.GroupBy(r => r.SecurityContextType.Id, r => r.SecurityContextId)
	//						.ToDictionary(
	//							g => securityContextInfoSource.GetSecurityContextInfo(g.Key).Type,
	//							Array (g) => g.ToArray())))
	//			.ToList());
	//}
}