using CommonFramework;

using SecuritySystem.Expanders;

namespace SecuritySystem.Services;

public class SecurityRolesIdentsResolver(
    ISecurityRuleExpander securityRuleExpander,
    ISecurityRoleSource securityRoleSource)
    : ISecurityRolesIdentsResolver
{
    public Dictionary<Type, Array> Resolve(DomainSecurityRule.RoleBaseSecurityRule securityRule, bool includeVirtual = false)
    {
        return securityRuleExpander.FullRoleExpand(securityRule)
                                   .SecurityRoles
                                   .Select(securityRoleSource.GetSecurityRole)
                                   .Where(sr => includeVirtual || !sr.Information.IsVirtual)
                                   .Select(sr => sr.Identity)
                                   .GroupBy(i => i.IdentType, i => i.GetId())
                                   .ToDictionary(g => g.Key, g => g.ToArray(g.Key));
    }
}