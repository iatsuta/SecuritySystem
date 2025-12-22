// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public interface ISecurityContextInfoSource
{
    IReadOnlyList<SecurityContextInfo> SecurityContextInfoList { get; }

    SecurityContextInfo GetSecurityContextInfo(Type type);

    SecurityContextInfo GetSecurityContextInfo(string name);

	SecurityContextInfo GetSecurityContextInfo(SecurityIdentity identity);
}
