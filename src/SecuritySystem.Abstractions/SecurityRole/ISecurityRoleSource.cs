using System.Collections.Immutable;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public interface ISecurityRoleSource
{
    ImmutableArray<FullSecurityRole> SecurityRoles { get; }

    FullSecurityRole GetSecurityRole(SecurityRole securityRole);

    FullSecurityRole GetSecurityRole(string name);

    FullSecurityRole GetSecurityRole(SecurityIdentity identity);

    IEnumerable<FullSecurityRole> GetRealRoles();
}
