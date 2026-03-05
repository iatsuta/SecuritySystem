using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace SecuritySystem.SecurityRuleInfo;

public class ClientSecurityRuleResolver(
    IDomainSecurityRoleExtractor domainSecurityRoleExtractor,
    IClientSecurityRuleInfoSource clientSecurityRuleInfoSource) : IClientSecurityRuleResolver
{
    private readonly ConcurrentDictionary<SecurityRole, ImmutableArray<DomainSecurityRule.ClientSecurityRule>> cache = [];

    public ImmutableArray<DomainSecurityRule.ClientSecurityRule> Resolve(SecurityRole securityRole) =>
        this.cache.GetOrAdd(securityRole, _ =>
        {
            var request =

                from clientSecurityRuleInfo in clientSecurityRuleInfoSource.GetInfos()

                let roles = domainSecurityRoleExtractor.ExtractSecurityRoles(clientSecurityRuleInfo.Implementation)

                where roles.Contains(securityRole)

                select clientSecurityRuleInfo.Rule;

            return [..request];
        });
}