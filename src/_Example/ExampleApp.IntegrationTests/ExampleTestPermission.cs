using ExampleApp.Domain;

using SecuritySystem;
using SecuritySystem.Testing;

namespace ExampleApp.IntegrationTests;

public class ExampleTestPermission : TestPermissionBuilder
{
    public ExampleTestPermission(SecurityRole securityRole)
        : this()
    {
        this.SecurityRole = securityRole;
    }

    public ExampleTestPermission()
    {
    }

    public TypedSecurityIdentity<Guid>? BusinessUnit
    {
        get => base.GetSingle<BusinessUnit, Guid>();
        set => base.SetSingle<BusinessUnit, Guid>(value);
    }
}