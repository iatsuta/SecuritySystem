// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public interface ISecuritySystemFactory
{
    ISecuritySystem Create(SecurityRuleCredential securityRuleCredential);
}
