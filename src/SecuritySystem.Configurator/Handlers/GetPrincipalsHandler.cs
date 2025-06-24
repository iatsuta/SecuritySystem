﻿using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Configurator.Models;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Configurator.Handlers;

public class GetPrincipalsHandler([CurrentUserWithoutRunAs]ISecuritySystem securitySystem, IRootPrincipalSourceService principalSourceService)
    : BaseReadHandler, IGetPrincipalsHandler
{
    protected override async Task<object> GetDataAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (!securitySystem.IsSecurityAdministrator()) return new List<EntityDto>();

        var nameFilter = (string)context.Request.Query["searchToken"]!;

        var principals = await principalSourceService.GetPrincipalsAsync(nameFilter, 70, cancellationToken);

        return principals
               .Select(x => new PrincipalHeaderDto { Id = x.Id, Name = x.Name, IsVirtual = x.IsVirtual })
               .OrderBy(x => x.Name)
               .ToList();
    }
}
