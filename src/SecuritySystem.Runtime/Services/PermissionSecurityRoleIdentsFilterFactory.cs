using System.Collections.Concurrent;
using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.Services;

public class PermissionSecurityRoleIdentsFilterFactory<TPermission>(
    IPermissionSecurityRoleFilterFactory<TPermission> permissionSecurityRoleFilterFactory,
    ISecurityRolesIdentsResolver securityRolesIdentsResolver) : IPermissionSecurityRoleIdentsFilterFactory<TPermission>
{
    private readonly ConcurrentDictionary<DomainSecurityRule.RoleBaseSecurityRule, Expression<Func<TPermission, bool>>> cache = [];

    public Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        this.cache.GetOrAdd(securityRule.WithDefaultCustoms(), _ => this.GetFilterElements(securityRule).BuildAnd());

    private IEnumerable<Expression<Func<TPermission, bool>>> GetFilterElements(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        foreach (var (securityRoleIdentType, securityRoleIdents) in securityRolesIdentsResolver.Resolve(securityRule))
        {
            yield return permissionSecurityRoleFilterFactory.CreateFilter(securityRoleIdentType, securityRoleIdents);
        }
    }
}