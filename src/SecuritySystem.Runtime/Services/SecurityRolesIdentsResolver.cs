using System.Collections.Concurrent;
using System.Collections.Immutable;

using CommonFramework;

using SecuritySystem.Expanders;

namespace SecuritySystem.Services;

public class SecurityRolesIdentsResolver(ISecurityRuleExpander securityRuleExpander, ISecurityRoleSource securityRoleSource) : ISecurityRolesIdentsResolver
{
    private readonly ConcurrentDictionary<(DomainSecurityRule.RoleBaseSecurityRule, bool), ImmutableDictionary<Type, Array>> cache = new();

    public IReadOnlyDictionary<Type, Array> Resolve(DomainSecurityRule.RoleBaseSecurityRule baseSecurityRule, bool includeVirtual = false)
    {
        return this.cache.GetOrAdd((baseSecurityRule.WithDefaultCredential(), includeVirtual), pair =>

            securityRuleExpander
                .FullRoleExpand(pair.Item1)
                .Children
                .SelectMany(c => c.SecurityRoles)
                .Distinct()
                .Select(securityRoleSource.GetSecurityRole)
                .Where(sr => includeVirtual || !sr.Information.IsVirtual)
                .Select(sr => sr.Identity)
                .GroupBy(i => i.IdentType, i => i.GetId())
                .ToImmutableDictionary(g => g.Key, g => g.ToArray(g.Key)));
    }
}