using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using HierarchicalExpand;
using HierarchicalExpand.AncestorDenormalization;

using SecuritySystem.Credential;
using SecuritySystem.DomainServices;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Testing;

public class RootAuthManager(
    IServiceProvider rootServiceProvider,
    IServiceProxyFactory serviceProxyFactory,
    ITestingEvaluator<IQueryableSource> queryableSourceEvaluator,
    ITestingEvaluator<IGenericRepository> genericRepositoryEvaluator,
    ITestingEvaluator<IServiceProvider> serviceProviderEvaluator,
    IIdentityInfoSource identityInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource)
{
    public RootUserCredentialManager For(UserCredential? userCredential = null)
    {
        return serviceProxyFactory.Create<RootUserCredentialManager>(Tuple.Create(userCredential));
    }

    public async Task<List<TypedSecurityIdentity<TIdent>>> GetIdentityListAsync<TDomainObject, TIdent>(SecurityRule securityRule,
        CancellationToken cancellationToken)
        where TIdent : notnull
        where TDomainObject : class
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TDomainObject, TIdent>();

        return await serviceProviderEvaluator.EvaluateAsync(TestingScopeMode.Read, async serviceProvider =>
        {
            var securityProvider = serviceProvider.GetRequiredService<IDomainSecurityService<TDomainObject>>().GetSecurityProvider(securityRule);

            var queryableSource = serviceProvider.GetRequiredService<IQueryableSource>();

            var idents = await queryableSource.GetQueryable<TDomainObject>().Pipe(securityProvider.InjectFilter).Select(identityInfo.Id.Path)
                .GenericToListAsync(cancellationToken);

            return idents.Select(TypedSecurityIdentity.Create).ToList();
        });
    }

    public async Task<TypedSecurityIdentity<TIdent>> GetSecurityContextIdentityAsync<TSecurityContext, TIdent>(string name, CancellationToken cancellationToken)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TIdent>();
        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TSecurityContext>();

        var filter = visualIdentityInfo.Name.Path.Select(v => v == name);

        return await queryableSourceEvaluator.EvaluateAsync(TestingScopeMode.Read, async queryableSource =>
        {
            var securityContextId = await queryableSource.GetQueryable<TSecurityContext>().Where(filter).Select(identityInfo.Id.Path)
                .GenericSingleAsync(cancellationToken);

            return TypedSecurityIdentity.Create(securityContextId);
        });
    }

    public async Task<TypedSecurityIdentity<TIdent>> SaveSecurityContextAsync<TSecurityContext, TIdent>(Func<TSecurityContext> createFunc,
        CancellationToken cancellationToken)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TIdent>();

        var id = await genericRepositoryEvaluator.EvaluateAsync(TestingScopeMode.Write, async genericRepository =>
        {
            var securityContext = createFunc();

            await genericRepository.SaveAsync(securityContext, cancellationToken);

            return identityInfo.Id.Getter(securityContext);
        });

        if (rootServiceProvider.GetService(typeof(FullAncestorLinkInfo<TSecurityContext>)) != null)
        {
            await serviceProviderEvaluator.EvaluateAsync(TestingScopeMode.Write, async serviceProvider =>
            {
                var queryableSource = serviceProvider.GetRequiredService<IQueryableSource>();

                var securityContext = await queryableSource.GetQueryable<TSecurityContext>()
                    .Where(identityInfo.Id.Path.Select(ExpressionHelper.GetEqualityWithExpr(id))).GenericSingleAsync(cancellationToken);

                var denormalizedAncestorsService = serviceProvider.GetRequiredService<IDenormalizedAncestorsService<TSecurityContext>>();

                await denormalizedAncestorsService.SyncUpAsync(securityContext, cancellationToken);
            });
        }

        return TypedSecurityIdentity.Create(id);
    }
}