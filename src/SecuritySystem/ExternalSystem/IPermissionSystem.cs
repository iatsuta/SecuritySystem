using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.ExternalSystem;

public interface IPermissionSystem
{
    Type PermissionType { get; }

    IPermissionSource GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule);

    Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken = default);
}

public interface IPermissionSystem<TPermission> : IPermissionSystem
{
    Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetPermissionRestrictionsExpr<TSecurityContext, TSecurityContextIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull;

    Expression<Func<TPermission, bool>> GetGrandAccessExpr<TSecurityContext, TSecurityContextIdent>()
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull;

	Expression<Func<TPermission, bool>> GetContainsIdentsExpr<TSecurityContext, TSecurityContextIdent>(IEnumerable<TSecurityContextIdent> idents,
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull =>
        this.GetPermissionRestrictionsExpr<TSecurityContext, TSecurityContextIdent>(restrictionFilterInfo)
            .Select(restrictionIdents => restrictionIdents.Any(restrictionIdent => idents.Contains(restrictionIdent)));

    new IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule);
}
