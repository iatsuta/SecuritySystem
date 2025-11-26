// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public interface ISecurityContextInfoSource
{
    IReadOnlyList<SecurityContextInfo> SecurityContextInfoList { get; }

    SecurityContextInfo GetSecurityContextInfo(Type securityContextType);

    SecurityContextInfo GetSecurityContextInfo(SecurityIdentity securityContextIdentity);
}
