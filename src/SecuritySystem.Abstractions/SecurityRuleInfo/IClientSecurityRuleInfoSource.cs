namespace SecuritySystem.SecurityRuleInfo;

public interface IClientSecurityRuleInfoSource
{
    IEnumerable<ClientSecurityRuleInfo> GetInfos();
}
