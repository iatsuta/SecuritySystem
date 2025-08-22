using CommonFramework;
using CommonFramework.DictionaryCache;

using Microsoft.Extensions.DependencyInjection;
using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class SecurityContextStorage : ISecurityContextStorage
{
    private readonly IServiceProvider serviceProvider;

    private readonly IIdentityInfoSource identityInfoSource;

    private readonly IDictionaryCache<Type, ITypedSecurityContextStorage> typedCache;


    public SecurityContextStorage(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource)
    {
        this.serviceProvider = serviceProvider;
        this.identityInfoSource = identityInfoSource;

        this.typedCache = new DictionaryCache<Type, ITypedSecurityContextStorage>(this.GetTypedInternal);
    }
    
    public ITypedSecurityContextStorage GetTyped(Type securityContextType)
    {
        return this.typedCache[securityContextType];
    }

    private ITypedSecurityContextStorage GetTypedInternal(Type securityContextType)
    {
        var identityType = identityInfoSource.GetIdentityInfo(securityContextType).IdentityType;

        return new Func<ITypedSecurityContextStorage>(this.GetTypedInternal<ISecurityContext, Ignore>).CreateGenericMethod(securityContextType, identityType)
            .Invoke<ITypedSecurityContextStorage>(this);
    }

    private ITypedSecurityContextStorage<TIdent> GetTypedInternal<TSecurityContext, TIdent>()
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull
    {
        var hierarchicalInfo = this.serviceProvider.GetService(typeof(HierarchicalInfo<>).MakeGenericType(typeof(TSecurityContext)));

        var typedSecurityContextStorageType =

            hierarchicalInfo != null

                ? typeof(HierarchicalTypedSecurityContextStorage<TSecurityContext, TIdent>)
                : typeof(PlainTypedSecurityContextStorage<TSecurityContext, TIdent>);

        var typedSecurityContextStorage = (ITypedSecurityContextStorage<TIdent>)ActivatorUtilities.CreateInstance(this.serviceProvider, typedSecurityContextStorageType);

        return typedSecurityContextStorage.WithCache();
    }
}