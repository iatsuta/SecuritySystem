using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionSystemFactory(IServiceProvider serviceProvider) : IPermissionSystemFactory
{
    public IPermissionSystem Create(SecurityRuleCredential securityRuleCredential) =>
        ActivatorUtilities.CreateInstance<GeneralPermissionSystem>(serviceProvider, securityRuleCredential);
}
