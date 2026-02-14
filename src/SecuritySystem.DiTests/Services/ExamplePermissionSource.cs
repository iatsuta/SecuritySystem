using CommonFramework;

using SecuritySystem.ExternalSystem;

namespace SecuritySystem.DiTests.Services;

public class ExamplePermissionSource(TestPermissions data, DomainSecurityRule.ExpandedRoleGroupSecurityRule securityRule) : IPermissionSource
{
    public bool HasAccess() => throw new NotImplementedException();

    public List<Dictionary<Type, Array>> GetPermissions(IEnumerable<Type> _)
    {
        var roles = securityRule.Children.SelectMany(c => c.SecurityRoles).ToHashSet();

        var request = from permission in data.Permissions

                      where roles.Contains(permission.SecurityRole)

                      select permission.Restrictions.ChangeValue(idents => (Array)idents.ToArray());

        return request.ToList();
    }
}