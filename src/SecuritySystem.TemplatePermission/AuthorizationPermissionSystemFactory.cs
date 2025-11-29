using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem;

namespace SecuritySystem.TemplatePermission;

public class TemplatePermissionSystemFactory(IServiceProvider serviceProvider) : IPermissionSystemFactory
{
    public IPermissionSystem Create(SecurityRuleCredential securityRuleCredential) =>
        ActivatorUtilities.CreateInstance<TemplatePermissionSystem>(serviceProvider, securityRuleCredential);
}
