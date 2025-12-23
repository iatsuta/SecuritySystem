using CommonFramework;
using CommonFramework.DictionaryCache;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public static class TypedSecurityContextStorageExtensions
{
    public static ITypedSecurityContextStorage<TIdent> WithCache<TIdent>(this ITypedSecurityContextStorage<TIdent> source)
        where TIdent : notnull
    {
        return new TypedSecurityEntitySource<TIdent>(source);
    }

    private class TypedSecurityEntitySource<TIdent>(ITypedSecurityContextStorage<TIdent> baseSource) : ITypedSecurityContextStorage<TIdent>
        where TIdent : notnull
    {
        private readonly Lazy<SecurityContextData<TIdent>[]> lazySecurityContexts = LazyHelper.Create(() => baseSource.GetSecurityContexts().ToArray());

        private readonly IDictionaryCache<TIdent[], SecurityContextData<TIdent>[]> securityEntitiesByIdentsCache = new DictionaryCache<TIdent[], SecurityContextData<TIdent>[]>(
                securityEntityIdents => baseSource.GetSecurityContextsByIdents(securityEntityIdents).ToArray(),
                ArrayComparer<TIdent>.Default);

        private readonly IDictionaryCache<TIdent, SecurityContextData<TIdent>[]> securityEntitiesWithMasterExpandCache = new DictionaryCache<TIdent, SecurityContextData<TIdent>[]>(
            startSecurityEntityId => baseSource.GetSecurityContextsWithMasterExpand(startSecurityEntityId).ToArray());

        private readonly IDictionaryCache<TIdent, bool> existsCache = new DictionaryCache<TIdent, bool>(baseSource.IsExists);

        private readonly IDictionaryCache<SecurityIdentity, bool> baseExistsCache = new DictionaryCache<SecurityIdentity, bool>(baseSource.IsExists);

        public IEnumerable<SecurityContextData<TIdent>> GetSecurityContexts()
        {
            return this.lazySecurityContexts.Value;
        }

        public IEnumerable<SecurityContextData<TIdent>> GetSecurityContextsByIdents(IEnumerable<TIdent> securityEntityIdents)
        {
            return this.securityEntitiesByIdentsCache[securityEntityIdents.ToArray()];
        }
        public IEnumerable<SecurityContextData<TIdent>> GetSecurityContextsWithMasterExpand(TIdent startSecurityEntityId)
        {
            return this.securityEntitiesWithMasterExpandCache[startSecurityEntityId];
        }

        public bool IsExists(TIdent securityEntityId)
        {
            return this.existsCache[securityEntityId];
        }
        public bool IsExists(SecurityIdentity securityIdentity)
        {
            return this.baseExistsCache[securityIdentity];
        }

        IEnumerable<SecurityContextData<object>> ITypedSecurityContextStorage.GetSecurityContextsByIdents(Array securityContextIdents)
        {
            return this.GetSecurityContextsByIdents(securityContextIdents.Cast<TIdent>()).Select(scd => scd.UpCast());
        }

        IEnumerable<SecurityContextData<object>> ITypedSecurityContextStorage.GetSecurityContexts()
        {
            return this.GetSecurityContexts().Select(scd => scd.UpCast());
        }
    }
}
