namespace SecuritySystem.Services;

public interface ISecurityRolesIdentsResolver
{
    IReadOnlyDictionary<Type, Array> Resolve(DomainSecurityRule.RoleBaseSecurityRule securityRule, bool includeVirtual = false);
}