using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Configurator.Models;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Configurator.Handlers;

public class GetOperationHandler(
    [CurrentUserWithoutRunAs]ISecuritySystem securitySystem,
    ISecurityRoleSource roleSource,
    IRootPrincipalSourceService principalSourceService)
    : BaseReadHandler, IGetOperationHandler
{
    protected override async Task<object> GetDataAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (!securitySystem.IsSecurityAdministrator()) return new OperationDetailsDto { BusinessRoles = [], Principals = [] };

        var operationName = (string)context.Request.RouteValues["name"]!;

        var securityRoles = roleSource.SecurityRoles
                                      .Where(x => x.Information.Operations.Any(o => o.Name == operationName))
                                      .ToList();

        var principals = await principalSourceService.GetLinkedPrincipalsAsync(securityRoles, cancellationToken);

        return new OperationDetailsDto
               {
                   BusinessRoles = securityRoles.Select(x => x.Name).Order().ToList(), Principals = principals.ToList()
               };
    }
}
