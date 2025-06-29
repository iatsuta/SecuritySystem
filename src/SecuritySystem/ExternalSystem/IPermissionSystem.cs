﻿using System.Linq.Expressions;
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
    Expression<Func<TPermission, IEnumerable<Guid>>> GetPermissionRestrictionsExpr<TSecurityContext>(SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext;

    Expression<Func<TPermission, bool>> GetGrandAccessExpr<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext =>

        this.GetPermissionRestrictionsExpr<TSecurityContext>(null).Select(v => !v.Any());

    Expression<Func<TPermission, bool>> GetContainsIdentsExpr<TSecurityContext>(IEnumerable<Guid> idents, SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext =>
        this.GetPermissionRestrictionsExpr(restrictionFilterInfo).Select(restrictionIdents => restrictionIdents.Any(restrictionIdent => idents.Contains(restrictionIdent)));

    new IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule);
}
