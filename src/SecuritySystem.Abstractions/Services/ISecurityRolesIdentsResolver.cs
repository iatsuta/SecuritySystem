namespace SecuritySystem.Services;

public interface ISecurityRolesIdentsResolver
{
    Array Resolve(DomainSecurityRule.RoleBaseSecurityRule securityRule, bool includeVirtual = false);
}