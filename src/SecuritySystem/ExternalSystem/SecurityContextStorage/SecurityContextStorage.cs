using CommonFramework;
using CommonFramework.DependencyInjection;
using CommonFramework.DictionaryCache;
using CommonFramework.IdentitySource;

using HierarchicalExpand;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class SecurityContextStorage : ISecurityContextStorage
{
    private readonly IServiceProvider serviceProvider;

    private readonly IServiceProxyFactory serviceProxyFactory;

    private readonly IIdentityInfoSource identityInfoSource;

    private readonly IDictionaryCache<Type, ITypedSecurityContextStorage> typedCache;


    public SecurityContextStorage(IServiceProvider serviceProvider, IServiceProxyFactory serviceProxyFactory,
        IIdentityInfoSource identityInfoSource)
    {
        this.serviceProvider = serviceProvider;
        this.serviceProxyFactory = serviceProxyFactory;
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

        var (serviceType, args) = hierarchicalInfo == null
            ? (typeof(PlainTypedSecurityContextStorage<TSecurityContext, TSecurityContextIdent>), Array.Empty<object>())
            : (typeof(HierarchicalTypedSecurityContextStorage<TSecurityContext, TSecurityContextIdent>), [hierarchicalInfo]);

        var typedSecurityContextStorage = serviceProxyFactory.Create<ITypedSecurityContextStorage<TSecurityContextIdent>>(serviceType, args);

        return typedSecurityContextStorage.WithCache();
    }
}