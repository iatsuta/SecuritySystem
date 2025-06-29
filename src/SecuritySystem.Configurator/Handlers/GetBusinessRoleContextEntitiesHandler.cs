﻿using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Configurator.Models;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.SecurityContextStorage;

namespace SecuritySystem.Configurator.Handlers;

public class GetBusinessRoleContextEntitiesHandler(
    ISecurityContextStorage securityContextStorage,
    [CurrentUserWithoutRunAs]ISecuritySystem securitySystem)
    : BaseReadHandler, IGetBusinessRoleContextEntitiesHandler
{
    protected override async Task<object> GetDataAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (!securitySystem.IsSecurityAdministrator()) return new List<EntityDto>();

        var securityContextTypeId = new Guid((string)context.Request.RouteValues["id"]!);
        var searchToken = context.Request.Query["searchToken"];

        var entities = securityContextStorage.GetTyped(securityContextTypeId).GetSecurityContexts();

        if (!string.IsNullOrWhiteSpace(searchToken))
            entities = entities.Where(p => p.Name.Contains(searchToken!, StringComparison.OrdinalIgnoreCase));

        return entities
               .Select(x => new EntityDto { Id = x.Id, Name = x.Name })
               .OrderByDescending(x => x.Name.Equals(searchToken, StringComparison.OrdinalIgnoreCase))
               .ThenBy(x => x.Name)
               .Take(70)
               .ToList();
    }
}
