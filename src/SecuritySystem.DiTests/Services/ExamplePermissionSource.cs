using CommonFramework;

using SecuritySystem.ExternalSystem;

namespace SecuritySystem.DiTests.Services;

public class ExamplePermissionSource(TestPermissionData data, DomainSecurityRule.ExpandedRolesSecurityRule securityRule) : IPermissionSource
{
    public bool HasAccess() => throw new NotImplementedException();

    public List<Dictionary<Type, List<Guid>>> GetPermissions(IEnumerable<Type> _)
    {
        var roles = securityRule.SecurityRoles.ToHashSet();

        var request = from permission in data.Permissions

                      where roles.Contains(permission.SecurityRole)

                      select permission.Restrictions.ChangeValue(idents => idents.ToList());

        return request.ToList();
    }
}