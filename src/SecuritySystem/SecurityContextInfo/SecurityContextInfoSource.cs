// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class SecurityContextInfoSource : ISecurityContextInfoSource
{
    private readonly IReadOnlyDictionary<Type, SecurityContextInfo> typeDict;

    private readonly IReadOnlyDictionary<SecurityIdentity, SecurityContextInfo> identityDict;

    public SecurityContextInfoSource(IEnumerable<SecurityContextInfo> securityContextInfoList)
    {
        this.SecurityContextInfoList = securityContextInfoList.ToList();
        this.typeDict = this.SecurityContextInfoList.ToDictionary(v => v.Type);
        this.identityDict = this.typeDict.Values.ToDictionary(v => v.Identity);
    }

    public IReadOnlyList<SecurityContextInfo> SecurityContextInfoList { get; }

    public virtual SecurityContextInfo GetSecurityContextInfo(Type securityContextType) =>
        this.typeDict[securityContextType];

    public SecurityContextInfo GetSecurityContextInfo(SecurityIdentity securityContextIdentity) =>
        this.identityDict[securityContextIdentity];
}
