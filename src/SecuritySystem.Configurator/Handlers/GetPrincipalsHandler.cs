using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Configurator.Models;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Configurator.Handlers;

public class GetPrincipalsHandler([WithoutRunAs] ISecuritySystem securitySystem, IRootPrincipalSourceService principalSourceService)
	: BaseReadHandler, IGetPrincipalsHandler
{
	protected override async Task<object> GetDataAsync(HttpContext context, CancellationToken cancellationToken)
	{
		if (!securitySystem.IsSecurityAdministrator()) return new List<EntityDto>();

        var nameFilter = context.ExtractSearchToken();

		var principals = await principalSourceService.GetPrincipalsAsync(nameFilter, 70, cancellationToken);

		return await principals
			.Select(x => new PrincipalHeaderDto { Id = x.Identity.GetId().ToString()!, Name = x.Name, IsVirtual = x.IsVirtual })
			.OrderBy(x => x.Name)
			.ToAsyncEnumerable()
			.ToListAsync(cancellationToken);
	}
}