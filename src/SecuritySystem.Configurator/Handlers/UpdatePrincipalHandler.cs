using Microsoft.AspNetCore.Http;

using SecuritySystem.Attributes;
using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Configurator.Handlers;

public class UpdatePrincipalHandler(
	[CurrentUserWithoutRunAs] ISecuritySystem securitySystem,
	IPrincipalManagementService principalManagementService,
	IConfiguratorIntegrationEvents? configuratorIntegrationEvents = null)
	: BaseWriteHandler, IUpdatePrincipalHandler
{
	public async Task Execute(HttpContext context, CancellationToken cancellationToken)
	{
		securitySystem.CheckAccess(ApplicationSecurityRule.SecurityAdministrator);

		var principalId = (string?)context.Request.RouteValues["id"]!;

		var principalName = await this.ParseRequestBodyAsync<string>(context);

		var principal = await principalManagementService.UpdatePrincipalNameAsync(new UserCredential.UntypedIdentUserCredential(principalId), principalName,
			cancellationToken);

		if (configuratorIntegrationEvents != null)
			await configuratorIntegrationEvents.PrincipalChangedAsync(principal, cancellationToken);
	}
}