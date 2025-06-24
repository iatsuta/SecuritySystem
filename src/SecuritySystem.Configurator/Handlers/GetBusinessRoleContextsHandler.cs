using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Configurator.Models;
using SecuritySystem.ExternalSystem.ApplicationSecurity;

namespace SecuritySystem.Configurator.Handlers;

public class GetBusinessRoleContextsHandler(
    ISecurityContextInfoSource securityContextInfoSource,
    [CurrentUserWithoutRunAs]ISecuritySystem securitySystem)
    : BaseReadHandler, IGetBusinessRoleContextsHandler
{
    protected override async Task<object> GetDataAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (!securitySystem.IsSecurityAdministrator()) return new List<EntityDto>();

        return securityContextInfoSource
               .SecurityContextInfoList
               .Select(x => new EntityDto { Id = x.Id, Name = x.Name })
               .OrderBy(x => x.Name)
               .ToList();
    }
}
