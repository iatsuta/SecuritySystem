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
    IIdentityInfoSource identityInfoSource,
    [CurrentUserWithoutRunAs] ISecuritySystem securitySystem) : BaseReadHandler, IGetPrincipalHandler
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
                    StartDate = typedPermission.StartDate,
                    EndDate = typedPermission.EndDate,
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

            group securityContextId by restrictionGroup.Key

            into g

            let identityType = identityInfoSource.GetIdentityInfo(g.Key).IdentityType

            let typedSecurityContextStorage = securityContextStorage.GetTyped(g.Key)

            let identsDict = (IReadOnlyDictionary<object, string>)typedSecurityContextStorage
                .GetSecurityContextsByIdents(g.Distinct().ToArray(identityType))
                .ToDictionary(scd => scd.Id, scd => scd.Name)

            select (g.Key, identsDict);

        return request.ToDictionary();
    }
}