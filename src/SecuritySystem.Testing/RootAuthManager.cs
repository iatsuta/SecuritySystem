using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Credential;

namespace SecuritySystem.Testing;

public class RootAuthManager(IServiceProvider rootServiceProvider, IIdentityInfoSource identityInfoSource, IVisualIdentityInfoSource visualIdentityInfoSource)
{
    public RootUserCredentialManager For(UserCredential? userCredential = null)
    {
        return new RootUserCredentialManager(rootServiceProvider, userCredential);
    }

    public async Task<TypedSecurityIdentity<TIdent>> GetSecurityContextIdentityAsync<TSecurityContext, TIdent>(string name, CancellationToken cancellationToken)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TIdent>();
        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TSecurityContext>();

        var filter = visualIdentityInfo.Name.Path.Select(v => v == name);

        await using var scope = rootServiceProvider.CreateAsyncScope();

        var queryableSource = scope.ServiceProvider.GetRequiredService<IQueryableSource>();

        var securityContext = await queryableSource.GetQueryable<TSecurityContext>().Where(filter).GenericSingleAsync(cancellationToken);

        return TypedSecurityIdentity.Create(identityInfo.Id.Getter(securityContext));
    }
}