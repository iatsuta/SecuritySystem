using CommonFramework;

using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Configurator.Models;
using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.ExternalSystem.SecurityContextStorage;

namespace SecuritySystem.Configurator.Handlers;

public class GetPrincipalHandler(
	IRootPrincipalSourceService principalSourceService,
	ISecurityContextStorage securityContextStorage,
	ISecurityRoleSource securityRoleSource,
	ISecurityContextInfoSource securityContextInfoSource,
	[WithoutRunAs] ISecuritySystem securitySystem) : BaseReadHandler, IGetPrincipalHandler
{
	protected override async Task<object> GetDataAsync(HttpContext httpContext, CancellationToken cancellationToken)
	{
		if (!securitySystem.IsSecurityAdministrator()) return new PrincipalDetailsDto();

		var principalId = (string)httpContext.Request.RouteValues["id"]!;

		var permissions = await this.GetPermissionsAsync(new UserCredential.UntypedIdentUserCredential(principalId), cancellationToken);

		return new PrincipalDetailsDto { Permissions = permissions };
	}

	private async Task<List<PermissionDto>> GetPermissionsAsync(UserCredential userCredential, CancellationToken cancellationToken)
	{
		var principal = await principalSourceService.TryGetPrincipalAsync(userCredential, cancellationToken)
		                ?? throw new SecuritySystemException($"Principal with id {userCredential} not found");

		var allSecurityContextDict = this.GetSecurityContextDict(principal);

		return principal
			.Permissions
			.Select(typedPermission =>
				new PermissionDto
				{
					Id = typedPermission.Id,
					IsVirtual = typedPermission.IsVirtual,
					Role = typedPermission.SecurityRole.Name,
					RoleId = securityRoleSource.GetSecurityRole(typedPermission.SecurityRole).Identity.ToString()!,
					Comment = typedPermission.Comment,
					StartDate = typedPermission.Period.StartDate,
					EndDate = typedPermission.Period.EndDate,
					Contexts = typedPermission
						.Restrictions
						.Select(restriction =>
						{
							var typedCache = allSecurityContextDict[restriction.Key];

							var securityContextInfo = securityContextInfoSource.GetSecurityContextInfo(restriction.Key);

							return new ContextDto
							{
								Id = securityContextInfo.Identity.ToString()!,
								Name = securityContextInfo.Name,
								Entities = restriction.Value.Cast<object>().Select(securityContextId =>
										new RestrictionDto { Id = securityContextId.ToString()!, Name = typedCache[securityContextId] })
									.ToList()
							};
						})
						.ToList()
				})
			.ToList();
	}

	private IReadOnlyDictionary<Type, IReadOnlyDictionary<object, string>> GetSecurityContextDict(TypedPrincipal principal)
	{
		var request =

			from permission in principal.Permissions

			from restrictionGroup in permission.Restrictions

			from object securityContextId in restrictionGroup.Value

			group securityContextId by new { SecurityContextType = restrictionGroup.Key, IdentType = restrictionGroup.Value.GetType().GetElementType()! }

			into g

			let typedSecurityContextStorage = securityContextStorage.GetTyped(g.Key.SecurityContextType)

			let identsDict = (IReadOnlyDictionary<object, string>)typedSecurityContextStorage
				.GetSecurityContextsByIdents(g.Distinct().ToArray(g.Key.IdentType))
				.ToDictionary(scd => scd.Id, scd => scd.Name)

			select (g.Key.SecurityContextType, identsDict);

		return request.ToDictionary();
	}
}