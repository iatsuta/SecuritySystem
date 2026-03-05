using System.Collections.Immutable;

namespace SecuritySystem.SecurityRuleInfo;

public interface IDomainSecurityRoleExtractor
{
    ImmutableHashSet<SecurityRole> ExtractSecurityRoles(DomainSecurityRule securityRule);

    DomainSecurityRule.ExpandedRoleGroupSecurityRule ExtractSecurityRule(DomainSecurityRule securityRule);
}
