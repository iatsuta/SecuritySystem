using CommonFramework.DictionaryCache;




namespace SecuritySystem.Expanders;

public class SecurityOperationExpander(ISecurityRoleSource securityRoleSource, ISecurityOperationInfoSource securityOperationInfoSource)
    : ISecurityOperationExpander
{
    private readonly IDictionaryCache<DomainSecurityRule.OperationSecurityRule, DomainSecurityRule.NonExpandedRolesSecurityRule> cache =
        new DictionaryCache<DomainSecurityRule.OperationSecurityRule, DomainSecurityRule.NonExpandedRolesSecurityRule>(
            securityRule =>
            {
                var securityRoles = securityRoleSource.SecurityRoles
                                                      .Where(sr => sr.Information.Operations.Contains(securityRule.SecurityOperation))
                                                      .Distinct()
                                                      .ToArray();

                if (securityRoles.Length == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(securityRule), $"No security roles found for operation \"{securityRule.SecurityOperation}\"");
                }

                return securityRoles.ToSecurityRule(
                    securityRule.CustomExpandType
                    ?? securityOperationInfoSource.GetSecurityOperationInfo(securityRule.SecurityOperation).CustomExpandType,
                    securityRule.CustomCredential,
                    securityRule.CustomRestriction);

            }).WithLock();


    public DomainSecurityRule.NonExpandedRolesSecurityRule Expand(DomainSecurityRule.OperationSecurityRule securityRule)
    {
        return this.cache[securityRule];
    }
}
