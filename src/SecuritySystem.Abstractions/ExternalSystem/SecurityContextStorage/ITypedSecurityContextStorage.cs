namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public interface ITypedSecurityContextStorage
{
    IEnumerable<SecurityContextData<object>> GetSecurityContexts();

    IEnumerable<SecurityContextData<object>> GetSecurityContextsByIdents(Array securityContextIdents);
}

public interface ITypedSecurityContextStorage<TIdent> : ITypedSecurityContextStorage
    where TIdent : notnull
{
    new IEnumerable<SecurityContextData<TIdent>> GetSecurityContexts();

    IEnumerable<SecurityContextData<TIdent>> GetSecurityContextsByIdents(IEnumerable<TIdent> securityContextIdents);

    IEnumerable<SecurityContextData<TIdent>> GetSecurityContextsWithMasterExpand(TIdent startSecurityEntityId);

    bool IsExists (TIdent securityContextId);
}
