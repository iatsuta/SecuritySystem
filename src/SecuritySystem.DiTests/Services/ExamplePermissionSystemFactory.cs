using SecuritySystem.Expanders;
using SecuritySystem.ExternalSystem;

namespace SecuritySystem.DiTests.Services;

public class ExamplePermissionSystemFactory(ISecurityRuleExpander securityRuleExpander, TestPermissionData data) : IPermissionSystemFactory
{
    public IPermissionSystem Create(SecurityRuleCredential securityRuleCredential) =>
        new ExamplePermissionSystem(securityRuleExpander, data);
}
