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
    Expression<Func<TPermission, IEnumerable<TIdent>>> GetPermissionRestrictionsExpr<TSecurityContext, TIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull;

    Expression<Func<TPermission, bool>> GetGrandAccessExpr<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext;

    Expression<Func<TPermission, bool>> GetContainsIdentsExpr<TSecurityContext, TIdent>(IEnumerable<TIdent> idents,
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull =>
        this.GetPermissionRestrictionsExpr<TSecurityContext, TIdent>(restrictionFilterInfo)
            .Select(restrictionIdents => restrictionIdents.Any(restrictionIdent => idents.Contains(restrictionIdent)));

    new IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule);
}
