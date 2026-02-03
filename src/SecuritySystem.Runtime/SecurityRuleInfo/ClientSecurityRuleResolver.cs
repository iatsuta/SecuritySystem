using System.Collections.Concurrent;

namespace SecuritySystem.SecurityRuleInfo;

public class ClientSecurityRuleResolver(
    IDomainSecurityRoleExtractor domainSecurityRoleExtractor,
    IClientSecurityRuleInfoSource clientSecurityRuleInfoSource) : IClientSecurityRuleResolver
{
    private readonly ConcurrentDictionary<SecurityRole, DomainSecurityRule.ClientSecurityRule[]> cache = new();

    public IEnumerable<DomainSecurityRule.ClientSecurityRule> Resolve(SecurityRole securityRole) =>
        this.cache.GetOrAdd(securityRole, _ =>
        {
            var request = from clientSecurityRuleInfo in clientSecurityRuleInfoSource.GetInfos()

                let roles = domainSecurityRoleExtractor.ExtractSecurityRoles(clientSecurityRuleInfo.Implementation)

                where roles.Contains(securityRole)

                select clientSecurityRuleInfo.Rule;

            return request.ToArray();
        });
}