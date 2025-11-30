// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class SecurityContextInfoSource : ISecurityContextInfoSource
{
    private readonly IReadOnlyDictionary<Type, SecurityContextInfo> typeDict;

    private readonly IReadOnlyDictionary<SecurityIdentity, SecurityContextInfo> identityDict;


    private readonly IReadOnlyDictionary<string, SecurityContextInfo> nameDict;

	public SecurityContextInfoSource(IEnumerable<SecurityContextInfo> securityContextInfoList)
    {
        this.SecurityContextInfoList = securityContextInfoList.ToList();
        this.typeDict = this.SecurityContextInfoList.ToDictionary(v => v.Type);
        this.identityDict = this.typeDict.Values.ToDictionary(v => v.Identity);
        this.nameDict = this.typeDict.Values.ToDictionary(v => v.Name);
	}

    public IReadOnlyList<SecurityContextInfo> SecurityContextInfoList { get; }

    public virtual SecurityContextInfo GetSecurityContextInfo(Type securityContextType) =>
        this.typeDict[securityContextType];

    public SecurityContextInfo GetSecurityContextInfo(string securityContextTypeName) =>
	    this.nameDict[securityContextTypeName];

	public SecurityContextInfo GetSecurityContextInfo(SecurityIdentity securityContextIdentity) =>
        this.identityDict[securityContextIdentity];
}
