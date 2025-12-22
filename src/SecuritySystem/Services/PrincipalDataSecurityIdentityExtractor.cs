using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Services;

public class PrincipalDataSecurityIdentityExtractor(IServiceProvider serviceProvider) : IPrincipalDataSecurityIdentityExtractor
{
    private readonly ConcurrentDictionary<Type, IPrincipalDataSecurityIdentityExtractor> cache = new();

    public TypedSecurityIdentity Extract(PrincipalData principalData)
    {
        return this.cache.GetOrAdd(principalData.PrincipalType, _ =>
        {
            var serviceType = typeof(PrincipalDataSecurityIdentityExtractor<>).MakeGenericType(principalData.PrincipalType);

            return (IPrincipalDataSecurityIdentityExtractor)ActivatorUtilities.CreateInstance(serviceProvider, serviceType);
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