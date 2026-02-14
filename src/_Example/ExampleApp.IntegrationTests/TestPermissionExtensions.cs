using ExampleApp.Domain;

using SecuritySystem;
using SecuritySystem.Testing;

namespace ExampleApp.IntegrationTests;

public static class TestPermissionExtensions
{
    extension(TestPermission testPermission)
    {
        public TypedSecurityIdentity<Guid>? BusinessUnit
        {
            get => testPermission.GetSingle<BusinessUnit, Guid>();
            set => testPermission.SetSingle<BusinessUnit, Guid>(value);
        }

        public TypedSecurityIdentity<Guid>[] BusinessUnits
        {
            get => testPermission.GetMany<BusinessUnit, Guid>();
            set => testPermission.SetMany<BusinessUnit, Guid>(value);
        }
    }
}
