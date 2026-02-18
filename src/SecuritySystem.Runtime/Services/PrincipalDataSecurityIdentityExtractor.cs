using System.Collections.Concurrent;

using CommonFramework;

using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Services;

public class PrincipalDataSecurityIdentityExtractor(IServiceProxyFactory serviceProxyFactory) : IPrincipalDataSecurityIdentityExtractor
{
    private readonly ConcurrentDictionary<Type, IPrincipalDataSecurityIdentityExtractor> cache = [];

    public TypedSecurityIdentity Extract(PrincipalData principalData)
    {
        return this.cache.GetOrAdd(principalData.PrincipalType, _ =>
        {
            var serviceType = typeof(PrincipalDataSecurityIdentityExtractor<>).MakeGenericType(principalData.PrincipalType);

            return serviceProxyFactory.Create<IPrincipalDataSecurityIdentityExtractor>(serviceType);
        }).Extract(principalData);
    }
}

public class PrincipalDataSecurityIdentityExtractor<TPrincipal>(ISecurityIdentityExtractor<TPrincipal> securityIdentityExtractor)
    : IPrincipalDataSecurityIdentityExtractor
{
    public TypedSecurityIdentity Extract(PrincipalData principalData)
    {
        var typedPrincipalData = (PrincipalData<TPrincipal>)principalData;

        return securityIdentityExtractor.Extract(typedPrincipalData.Principal);
    }
}