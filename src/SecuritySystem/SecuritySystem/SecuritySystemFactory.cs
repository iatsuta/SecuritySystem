using SecuritySystem.ExternalSystem;
using SecuritySystem.SecurityRuleInfo;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class SecuritySystemFactory(
    IAccessDeniedExceptionService accessDeniedExceptionService,
    IDomainSecurityRoleExtractor domainSecurityRoleExtractor,
    IEnumerable<IPermissionSystemFactory> permissionSystems) : ISecuritySystemFactory
{
    public ISecuritySystem Create(SecurityRuleCredential securityRuleCredential)
    {
        return new SecuritySystem(accessDeniedExceptionService, permissionSystems.Select(f => f.Create(securityRuleCredential)).ToList(), domainSecurityRoleExtractor);
    }
}
