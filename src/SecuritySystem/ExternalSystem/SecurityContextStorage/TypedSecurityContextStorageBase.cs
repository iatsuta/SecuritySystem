using SecuritySystem.Services;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public abstract class TypedSecurityContextStorageBase<TSecurityContext, TIdent>(
    IQueryableSource queryableSource,
    LocalStorage<TSecurityContext, TIdent> localStorage,
    IdentityInfo<TSecurityContext, TIdent> identityInfo)
    : ITypedSecurityContextStorage<TIdent>
    where TSecurityContext : class, ISecurityContext
    where TIdent : notnull
{
    protected abstract SecurityContextData<TIdent> CreateSecurityContextData(TSecurityContext securityContext);

    public IEnumerable<SecurityContextData<TIdent>> GetSecurityContexts()
    {
        return queryableSource.GetQueryable<TSecurityContext>().ToList().Select(this.CreateSecurityContextData);
    }

    public IEnumerable<SecurityContextData<TIdent>> GetSecurityContextsByIdents(IEnumerable<TIdent> preSecurityEntityIdents)
    {
        var filter = identityInfo.CreateContainsFilter(preSecurityEntityIdents.ToArray());

        return queryableSource.GetQueryable<TSecurityContext>().Where(filter).ToList().Select(this.CreateSecurityContextData);
    }

    public IEnumerable<SecurityContextData<TIdent>> GetSecurityContextsWithMasterExpand(TIdent startSecurityEntityId)
    {
        var filter = identityInfo.CreateContainsFilter([startSecurityEntityId]);

        var securityObject = queryableSource.GetQueryable<TSecurityContext>().Single(filter);

        return this.GetSecurityContextsWithMasterExpand(securityObject).Select(this.CreateSecurityContextData);
    }

    public bool IsExists(TIdent securityContextId)
    {
        var filter = identityInfo.CreateContainsFilter([securityContextId]);

        return localStorage.IsExists(securityContextId)
               || queryableSource.GetQueryable<TSecurityContext>().Any(filter);
    }

    protected abstract IEnumerable<TSecurityContext> GetSecurityContextsWithMasterExpand(TSecurityContext startSecurityObject);
}
