namespace SecuritySystem.ExternalSystem;

public interface IPermissionSystem
{
    Type PermissionType { get; }

    IEnumerable<IPermissionSource> GetPermissionSources(DomainSecurityRule.RoleBaseSecurityRule securityRule);

    IAsyncEnumerable<SecurityRole> GetAvailableSecurityRoles();
}

public interface IPermissionSystem<TPermission> : IPermissionSystem
{
    IPermissionRestrictionSource<TPermission, TSecurityContextIdent> GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull;

    new IEnumerable<IPermissionSource<TPermission>> GetPermissionSources(DomainSecurityRule.RoleBaseSecurityRule securityRule);

    IEnumerable<IPermissionSource> IPermissionSystem.GetPermissionSources(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        this.GetPermissionSources(securityRule);
}