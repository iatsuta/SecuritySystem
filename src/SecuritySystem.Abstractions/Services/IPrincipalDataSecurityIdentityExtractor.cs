using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Services;

public interface IPrincipalDataSecurityIdentityExtractor
{
    TypedSecurityIdentity Extract(PrincipalData principalData);
}