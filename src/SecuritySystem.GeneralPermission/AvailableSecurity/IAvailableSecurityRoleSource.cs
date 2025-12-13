namespace SecuritySystem.GeneralPermission.AvailableSecurity;

public interface IAvailableSecurityRoleSource
{
    Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(SecurityRuleCredential securityRuleCredential, CancellationToken cancellationToken);
}