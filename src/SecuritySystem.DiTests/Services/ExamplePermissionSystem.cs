using SecuritySystem.Expanders;
using SecuritySystem.ExternalSystem;

namespace SecuritySystem.DiTests.Services;

public class ExamplePermissionSystem(ISecurityRuleExpander securityRuleExpander, TestPermissions data) : IPermissionSystem
{
    public Type PermissionType => throw new NotImplementedException();

    public IPermissionSource GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule) => new ExamplePermissionSource(data, securityRuleExpander.FullRoleExpand(securityRule));

    public async Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken = default) => data.Permissions.Select(p => p.SecurityRole);
}
