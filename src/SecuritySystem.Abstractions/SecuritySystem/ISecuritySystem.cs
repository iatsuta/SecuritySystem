// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public interface ISecuritySystem
{
    bool HasAccess(DomainSecurityRule securityRule);

    bool IsAdministrator() => this.HasAccess(SecurityRole.Administrator);

    void CheckAccess(DomainSecurityRule securityRule);
}
