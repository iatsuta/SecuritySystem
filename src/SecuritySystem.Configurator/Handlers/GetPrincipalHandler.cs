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
	protected override async Task<object> GetDataAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (!securitySystem.IsSecurityAdministrator()) return new PrincipalDetailsDto { Permissions = [] };

		var permissions = await this.GetPermissionsAsync(context.ExtractSecurityIdentity(), cancellationToken);

		return new PrincipalDetailsDto { Permissions = permissions };
	}

	private async Task<List<PermissionDto>> GetPermissionsAsync(UserCredential userCredential, CancellationToken cancellationToken)
	{
		var principal = await principalSourceService.TryGetPrincipalAsync(userCredential, cancellationToken)
		                ?? throw new SecuritySystemException($"Principal with id {userCredential} not found");

		var allSecurityContextDict = this.GetSecurityContextDict(principal);

		return principal
			.Permissions
			.Select(permission =>
				new PermissionDto
				{
					Id = permission.Identity.GetId().ToString()!,
					IsVirtual = permission.IsVirtual,
					Role = permission.SecurityRole.Name,
					RoleId = securityRoleSource.GetSecurityRole(permission.SecurityRole).Identity.GetId().ToString()!,
					Comment = permission.Comment,
					StartDate = permission.Period.StartDate,
					EndDate = permission.Period.EndDate,
					Contexts = permission
                        .Restrictions
						.Select(restriction =>
						{
							var typedCache = allSecurityContextDict[restriction.Key];

							var securityContextInfo = securityContextInfoSource.GetSecurityContextInfo(restriction.Key);

							return new ContextDto
							{
								Id = securityContextInfo.Identity.GetId().ToString()!,
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

	private Dictionary<Type, Dictionary<object, string>> GetSecurityContextDict(ManagedPrincipal principal)
	{
		var request =

			from permission in principal.Permissions

			from restrictionGroup in permission.Restrictions

			from object securityContextId in restrictionGroup.Value

			group securityContextId by new { SecurityContextType = restrictionGroup.Key, IdentType = restrictionGroup.Value.GetType().GetElementType()! }

			into g

			let typedSecurityContextStorage = securityContextStorage.GetTyped(g.Key.SecurityContextType)

			let identsDict = typedSecurityContextStorage
				.GetSecurityContextsByIdents(g.Distinct().ToArray(g.Key.IdentType))
				.ToDictionary(scd => scd.Id, scd => scd.Name)

			select (g.Key.SecurityContextType, identsDict);

		return request.ToDictionary();
	}
}