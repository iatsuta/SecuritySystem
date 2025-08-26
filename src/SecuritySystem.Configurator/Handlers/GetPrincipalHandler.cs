using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Configurator.Models;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.ExternalSystem.SecurityContextStorage;
using SecuritySystem.Services;

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

        var principalId = new Guid((string)httpContext.Request.RouteValues["id"]!);

        var permissions = await this.GetPermissionsAsync(principalId, cancellationToken);

        return new PrincipalDetailsDto { Permissions = permissions };
    }

    private async Task<List<PermissionDto>> GetPermissionsAsync(Guid principalId, CancellationToken cancellationToken)
    {
        var principal = await principalSourceService.TryGetPrincipalAsync(principalId, cancellationToken)
                        ?? throw new SecuritySystemException($"Principal with id {principalId} not found");

        var allSecurityContextDict = this.GetSecurityContextDict(principal);

        return principal
            .Permissions
            .Select(typedPermission =>
                new PermissionDto
                {
                    Id = typedPermission.Id,
                    IsVirtual = typedPermission.IsVirtual,
                    Role = typedPermission.SecurityRole.Name,
                    RoleId = securityRoleSource.GetSecurityRole(typedPermission.SecurityRole).Id,
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
                                Id = securityContextInfo.Id,
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