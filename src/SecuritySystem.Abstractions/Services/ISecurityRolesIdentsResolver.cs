namespace SecuritySystem.Services;

public interface ISecurityRolesIdentsResolver
{
    IEnumerable<Guid> Resolve(DomainSecurityRule.RoleBaseSecurityRule securityRule, bool includeVirtual = false);
}