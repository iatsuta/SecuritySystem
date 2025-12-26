using SecuritySystem.Services;

using System.Collections.Concurrent;

using CommonFramework.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class SecurityContextInfoSource : ISecurityContextInfoSource
{
    private readonly IReadOnlyDictionary<Type, SecurityContextInfo> typeDict;

    private readonly IReadOnlyDictionary<TypedSecurityIdentity, SecurityContextInfo> identityDict;

    private readonly IReadOnlyDictionary<string, SecurityContextInfo> nameDict;


    private readonly ConcurrentDictionary<SecurityIdentity, SecurityContextInfo> baseIdentityCache = new();

    private readonly ISecurityIdentityConverter rootIdentityConverter;

    public SecurityContextInfoSource(IServiceProxyFactory serviceProxyFactory, IEnumerable<SecurityContextInfo> securityContextInfoList)
    {
        this.SecurityContextInfoList = securityContextInfoList.ToList();

        this.typeDict = this.SecurityContextInfoList.ToDictionary(v => v.Type);
        this.identityDict = this.typeDict.Values.ToDictionary(v => v.Identity);
        this.nameDict = this.typeDict.Values.ToDictionary(v => v.Name);

        this.rootIdentityConverter =
            serviceProxyFactory.Create<ISecurityIdentityConverter, RootSecurityIdentityConverter>(
                this.SecurityContextInfoList.Select(sr => sr.Identity.IdentType).Distinct());
    }

    public IReadOnlyList<SecurityContextInfo> SecurityContextInfoList { get; }

    public virtual SecurityContextInfo GetSecurityContextInfo(Type type) =>
        this.typeDict[type];

    public SecurityContextInfo GetSecurityContextInfo(string name) =>
        this.nameDict[name];

    public SecurityContextInfo GetSecurityContextInfo(SecurityIdentity identity)
    {
        return this.baseIdentityCache.GetOrAdd(identity, _ =>
        {
            var convertedIdentity = rootIdentityConverter.TryConvert(identity);

            if (convertedIdentity != null && this.identityDict.TryGetValue(convertedIdentity, out var securityContextInfo))
            {
                return securityContextInfo;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(identity), $"{nameof(SecurityContextInfo)} with {nameof(identity)} '{identity}' not found");
            }
        });
    }
}