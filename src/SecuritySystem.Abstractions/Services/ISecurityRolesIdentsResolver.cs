using System.Collections.Immutable;

namespace SecuritySystem.Services;

public interface ISecurityRolesIdentsResolver
{
    ImmutableDictionary<Type, Array> Resolve(DomainSecurityRule.RoleBaseSecurityRule securityRule, bool includeVirtual = false);
}