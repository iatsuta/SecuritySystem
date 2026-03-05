using SecuritySystem.Expanders;
using SecuritySystem.ExternalSystem;

namespace SecuritySystem.DiTests.Services;

public class ExamplePermissionSystem(ISecurityRuleExpander securityRuleExpander, TestPermissions data) : IPermissionSystem
{
    public Type PermissionType => throw new NotImplementedException();

    public IEnumerable<IPermissionSource> GetPermissionSources(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        [new ExamplePermissionSource(data, securityRuleExpander.FullRoleExpand(securityRule))];

    public IAsyncEnumerable<SecurityRole> GetAvailableSecurityRoles() => data.Permissions.Select(p => p.SecurityRole).ToAsyncEnumerable();
}