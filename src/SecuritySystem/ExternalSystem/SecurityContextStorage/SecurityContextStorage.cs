using CommonFramework.DictionaryCache;

using Microsoft.Extensions.DependencyInjection;
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class SecurityContextStorage : ISecurityContextStorage
{
    private readonly IServiceProvider serviceProvider;

    private readonly ISecurityContextInfoSource securityContextInfoSource;

    private readonly IDictionaryCache<Type, ITypedSecurityContextStorage> typedCache;


    public SecurityContextStorage(IServiceProvider serviceProvider, ISecurityContextInfoSource securityContextInfoSource)
    {
        this.serviceProvider = serviceProvider;
        this.securityContextInfoSource = securityContextInfoSource;

        this.typedCache = new DictionaryCache<Type, ITypedSecurityContextStorage>(this.GetTypedInternal);
    }

    public ITypedSecurityContextStorage GetTyped(Guid securityContextTypeId)
    {
        return this.GetTyped(this.securityContextInfoSource.GetSecurityContextInfo(securityContextTypeId).Type);
    }

    public ITypedSecurityContextStorage GetTyped(Type securityContextType)
    {
        return this.typedCache[securityContextType];
    }

    private ITypedSecurityContextStorage GetTypedInternal(Type securityContextType)
    {
        var hierarchicalInfo = this.serviceProvider.GetService(typeof(HierarchicalInfo<>).MakeGenericType(securityContextType));

        var authorizationTypedExternalSourceType =

            hierarchicalInfo == null

            ? typeof(HierarchicalTypedSecurityContextStorage<>)
            : typeof(PlainTypedSecurityContextStorage<>);

        var authorizationTypedExternalSourceImplType = authorizationTypedExternalSourceType.MakeGenericType(securityContextType);

        var result = (ITypedSecurityContextStorage)
            ActivatorUtilities.CreateInstance(this.serviceProvider, authorizationTypedExternalSourceImplType);

        return result.WithCache();
    }
}