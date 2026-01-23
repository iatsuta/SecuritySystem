using CommonFramework;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Expanders;

public class RoleFactorySecurityRuleExpander(IServiceProvider serviceProvider) : IRoleFactorySecurityRuleExpander
{
    public DomainSecurityRule.RoleBaseSecurityRule Expand(DomainSecurityRule.RoleFactorySecurityRule securityRule)
    {
        var factory = (IFactory<DomainSecurityRule.RoleBaseSecurityRule>)serviceProvider.GetRequiredService(securityRule.RoleFactoryType);

        return factory.Create().ApplyCustoms(securityRule);
    }
}
