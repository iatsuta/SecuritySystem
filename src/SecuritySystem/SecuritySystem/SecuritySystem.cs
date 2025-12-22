using SecuritySystem.AccessDenied;
using SecuritySystem.ExternalSystem;
using SecuritySystem.Providers;
using SecuritySystem.SecurityRuleInfo;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class SecuritySystem(
    IAccessDeniedExceptionService accessDeniedExceptionService,
    IReadOnlyList<IPermissionSystem> permissionSystems,
    IDomainSecurityRoleExtractor domainSecurityRoleExtractor) : ISecuritySystem
{
    public bool HasAccess(DomainSecurityRule securityRule)
    {
        return this.HasAccess(domainSecurityRoleExtractor.ExtractSecurityRule(securityRule));
    }

    public void CheckAccess(DomainSecurityRule securityRule)
    {
        this.CheckAccess(domainSecurityRoleExtractor.ExtractSecurityRule(securityRule));
    }

    private bool HasAccess(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        return permissionSystems.Any(v => v.GetPermissionSource(securityRule).HasAccess());
    }

    private void CheckAccess(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        if (!this.HasAccess(securityRule))
        {
            throw accessDeniedExceptionService.GetAccessDeniedException(
                new AccessResult.AccessDeniedResult { SecurityRule = securityRule });
        }
    }
}
