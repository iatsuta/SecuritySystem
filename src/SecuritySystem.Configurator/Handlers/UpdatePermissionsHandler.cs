using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

namespace SecuritySystem.Configurator.Handlers;

public class UpdatePermissionsHandler(
    [CurrentUserWithoutRunAs] ISecuritySystem securitySystem,
    ISecurityRoleSource securityRoleSource,
    ISecurityContextInfoSource securityContextInfoSource,
    IDomainObjectIdentsParser domainObjectIdentsParser,
	IPrincipalManagementService principalManagementService,
    IConfiguratorIntegrationEvents? configuratorIntegrationEvents = null) : BaseWriteHandler, IUpdatePermissionsHandler
{
    public async Task Execute(HttpContext context, CancellationToken cancellationToken)
    {
        securitySystem.CheckAccess(ApplicationSecurityRule.SecurityAdministrator);

        var principalId = (string)context.Request.RouteValues["id"]!;
        var permissions = await this.ParseRequestBodyAsync<List<RequestBodyDto>>(context);

        var typedPermissions = permissions.Select(this.ToTypedPermission).ToList();

        var mergeResult = await principalManagementService.UpdatePermissionsAsync(new UserCredential.UntypedIdentUserCredential(principalId), typedPermissions, cancellationToken);

        if (configuratorIntegrationEvents != null)
        {
            foreach (var permission in mergeResult.AddingItems)
            {
                await configuratorIntegrationEvents.PermissionCreatedAsync(permission, cancellationToken);
            }

            foreach (var (permission, _) in mergeResult.CombineItems)
            {
                await configuratorIntegrationEvents.PermissionChangedAsync(permission, cancellationToken);
            }

            foreach (var permission in mergeResult.RemovingItems)
            {
                await configuratorIntegrationEvents.PermissionRemovedAsync(permission, cancellationToken);
            }
        }
    }

    private TypedPermission ToTypedPermission(RequestBodyDto permission)
    {
        var restrictionsRequest =

            from restriction in permission.Contexts

            let securityContextType = securityContextInfoSource.GetSecurityContextInfo(restriction.Name).Type

            let idents = domainObjectIdentsParser.Parse(securityContextType, restriction.Entities)

            select (securityContextType, idents);


        return new TypedPermission(
	        permission.PermissionId,
	        permission.IsVirtual,
	        securityRoleSource.GetSecurityRole(permission.RoleName),
	        (permission.StartDate, permission.EndDate),
	        permission.Comment,
	        restrictionsRequest.ToDictionary());
    }

    private class RequestBodyDto
    {
        public string PermissionId { get; set; } = default!;

        public bool IsVirtual { get; set; }

        public string RoleName { get; set; } = default!;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Comment { get; set; } = default!;

        public List<ContextDto> Contexts { get; set; } = default!;

        public class ContextDto
        {
            public string Name { get; set; } = default!;

            public List<string> Entities { get; set; } = default!;
        }
    }
}
