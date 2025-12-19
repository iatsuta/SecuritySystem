using System.Collections.Concurrent;

using CommonFramework.IdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Services;

public class SecurityIdentityExtractor(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource) : ISecurityIdentityExtractor
{
    private readonly ConcurrentDictionary<Type, ISecurityIdentityExtractor> cache = new();

    public TypedSecurityIdentity Extract(PrincipalData principalData)
    {
        return this.cache.GetOrAdd(principalData.PrincipalType, _ =>
        {
            var identityInfo = identityInfoSource.GetIdentityInfo(principalData.PrincipalType);

            var serviceType = typeof(SecurityIdentityExtractor<,>).MakeGenericType(principalData.PrincipalType, identityInfo.IdentityType);

            return (ISecurityIdentityExtractor)ActivatorUtilities.CreateInstance(serviceProvider, serviceType, identityInfo);
        }).Extract(principalData);
    }
}

public class SecurityIdentityExtractor<TPrincipal, TPrincipalIdent>(IdentityInfo<TPrincipal, TPrincipalIdent> identityInfo) : ISecurityIdentityExtractor
    where TPrincipalIdent : notnull
{
    public TypedSecurityIdentity Extract(PrincipalData principalData)
    {
        var typedPrincipalData = (PrincipalData<TPrincipal>)principalData;

        return TypedSecurityIdentity.Create(identityInfo.Id.Getter(typedPrincipalData.Principal));
    }
}