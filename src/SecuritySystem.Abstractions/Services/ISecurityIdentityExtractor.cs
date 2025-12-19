using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Services;

public interface ISecurityIdentityExtractor
{
    TypedSecurityIdentity Extract(PrincipalData principalData);
}