namespace SecuritySystem.ExternalSystem;

public interface IPermissionSystem
{
    Type PermissionType { get; }

    IPermissionSource GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule);

    Task<List<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken = default);
}

public interface IPermissionSystem<TPermission> : IPermissionSystem
{
    IPermissionRestrictionSource<TPermission, TSecurityContextIdent> GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull;

    new IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule);

    IPermissionSource IPermissionSystem.GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule) => this.GetPermissionSource(securityRule);
}