using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Configurator.Models;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Configurator.Handlers;

public class GetBusinessRoleHandler(
    [WithoutRunAs]ISecuritySystem securitySystem,
    ISecurityRoleSource securityRoleSource,
    ISecurityOperationInfoSource securityOperationInfoSource,
    IRootPrincipalSourceService principalSourceService)
    : BaseReadHandler, IGetBusinessRoleHandler
{
    protected override async Task<object> GetDataAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (!securitySystem.IsSecurityAdministrator()) return new BusinessRoleDetailsDto { Operations = [], Principals = [] };

        var securityRoleId = (string)context.Request.RouteValues["id"]!;

        var securityRole = securityRoleSource.GetSecurityRole(new UntypedSecurityIdentity(securityRoleId));

        var operations =
            securityRole
                .Information
                .Operations
                .Select(
                    o => new OperationDto
                         {
                             Name = o.Name, Description = securityOperationInfoSource.GetSecurityOperationInfo(o).Description
                         })
                .OrderBy(x => x.Name)
                .ToList();

        var principals = await principalSourceService.GetLinkedPrincipalsAsync([securityRole], cancellationToken);

        return new BusinessRoleDetailsDto { Operations = operations, Principals = principals.ToList() };
    }
}
