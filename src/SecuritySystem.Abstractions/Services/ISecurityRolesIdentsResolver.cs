namespace SecuritySystem.Services;

public interface ISecurityRolesIdentsResolver
{
    Dictionary<Type, Array> Resolve(DomainSecurityRule.RoleBaseSecurityRule securityRule, bool includeVirtual = false);
}