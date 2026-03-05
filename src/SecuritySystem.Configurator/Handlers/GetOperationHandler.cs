using System.Collections.Immutable;
using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Configurator.Models;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Configurator.Handlers;

public class GetOperationHandler(
    [WithoutRunAs] ISecuritySystem securitySystem,
    ISecurityRoleSource roleSource,
    IRootPrincipalSourceService principalSourceService)
    : BaseReadHandler, IGetOperationHandler
{
    protected override async Task<object> GetDataAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (!securitySystem.IsSecurityAdministrator()) return new OperationDetailsDto { BusinessRoles = [], Principals = [] };

        var securityOperation = new SecurityOperation(context.ExtractName());

        var securityRoles = roleSource.SecurityRoles
            .Where(x => x.Information.Operations.Contains(securityOperation))
            .ToImmutableHashSet<SecurityRole>();

        var principals = await principalSourceService.GetLinkedPrincipalsAsync(securityRoles).ToListAsync(cancellationToken);

        return new OperationDetailsDto
        {
            BusinessRoles = securityRoles.Select(x => x.Name).Order().ToList(), Principals = principals
        };
    }
}