using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Configurator.Models;
using SecuritySystem.ExternalSystem.ApplicationSecurity;

namespace SecuritySystem.Configurator.Handlers;

public class GetBusinessRoleContextsHandler(
	ISecurityContextInfoSource securityContextInfoSource,
	[WithoutRunAs] ISecuritySystem securitySystem)
	: BaseReadHandler, IGetBusinessRoleContextsHandler
{
	protected override async Task<object> GetDataAsync(HttpContext context, CancellationToken cancellationToken)
	{
		if (!securitySystem.IsSecurityAdministrator()) return new List<EntityDto>();

		return await securityContextInfoSource
			.SecurityContextInfoList
			.Select(x => new EntityDto { Id = x.Identity.GetId().ToString()!, Name = x.Name })
			.OrderBy(x => x.Name)
			.ToAsyncEnumerable()
			.ToListAsync(cancellationToken);
	}
}