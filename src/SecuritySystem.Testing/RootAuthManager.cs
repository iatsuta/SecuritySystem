using CommonFramework;
using CommonFramework.DependencyInjection;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using SecuritySystem.Credential;

namespace SecuritySystem.Testing;

public class RootAuthManager(
    IServiceProxyFactory serviceProxyFactory,
    ITestingEvaluator<IQueryableSource> queryableSourceEvaluator,
    IIdentityInfoSource identityInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource)
{
    public RootUserCredentialManager For(UserCredential? userCredential = null)
    {
        return serviceProxyFactory.Create<RootUserCredentialManager>(Tuple.Create(userCredential));
    }

    public async Task<TypedSecurityIdentity<TIdent>> GetSecurityContextIdentityAsync<TSecurityContext, TIdent>(string name, CancellationToken cancellationToken)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TIdent>();
        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TSecurityContext>();

        var filter = visualIdentityInfo.Name.Path.Select(v => v == name);

        return await queryableSourceEvaluator.EvaluateAsync(async queryableSource =>
        {
            var securityContext = await queryableSource.GetQueryable<TSecurityContext>().Where(filter).GenericSingleAsync(cancellationToken);

            return TypedSecurityIdentity.Create(identityInfo.Id.Getter(securityContext));
        });
    }
}