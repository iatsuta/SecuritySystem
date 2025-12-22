// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public interface ISecurityRoleSource
{
    IReadOnlyList<FullSecurityRole> SecurityRoles { get; }

    FullSecurityRole GetSecurityRole(SecurityRole securityRole);

    FullSecurityRole GetSecurityRole(string name);

    FullSecurityRole GetSecurityRole(SecurityIdentity identity);

    IEnumerable<FullSecurityRole> GetRealRoles();
}
