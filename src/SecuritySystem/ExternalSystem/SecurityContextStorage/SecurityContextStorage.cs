using CommonFramework;
using CommonFramework.DictionaryCache;
using CommonFramework.IdentitySource;
using HierarchicalExpand;
using Microsoft.Extensions.DependencyInjection;

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

    private ITypedSecurityContextStorage<TSecurityContextIdent> GetTypedInternal<TSecurityContext, TSecurityContextIdent>()
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull
    {
        var hierarchicalInfo = this.serviceProvider.GetService(typeof(HierarchicalInfo<>).MakeGenericType(typeof(TSecurityContext)));

        var untypedSecurityContextStorageType =

            hierarchicalInfo == null

                ? ActivatorUtilities.CreateInstance(this.serviceProvider, typeof(PlainTypedSecurityContextStorage<TSecurityContext, TSecurityContextIdent>))

                : ActivatorUtilities.CreateInstance(this.serviceProvider, typeof(HierarchicalTypedSecurityContextStorage<TSecurityContext, TSecurityContextIdent>), hierarchicalInfo);

        var typedSecurityContextStorage = (ITypedSecurityContextStorage<TSecurityContextIdent>)untypedSecurityContextStorageType;

        return typedSecurityContextStorage.WithCache();
    }
}