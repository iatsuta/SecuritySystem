using System.Reflection;

namespace SecuritySystem.SecurityRuleInfo;

public interface IClientSecurityRuleNameExtractor
{
    string ExtractName(PropertyInfo propertyInfo);

    string ExtractName(DomainSecurityRule.DomainModeSecurityRule securityRule);
}
