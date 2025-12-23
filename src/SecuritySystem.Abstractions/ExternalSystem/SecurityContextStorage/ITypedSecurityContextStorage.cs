namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public interface ITypedSecurityContextStorage
{
    IEnumerable<SecurityContextData<object>> GetSecurityContexts();

    IEnumerable<SecurityContextData<object>> GetSecurityContextsByIdents(Array securityContextIdents);

    bool IsExists(SecurityIdentity securityIdentity);
}

public interface ITypedSecurityContextStorage<TSecurityContextIdent> : ITypedSecurityContextStorage
    where TSecurityContextIdent : notnull
{
    new IEnumerable<SecurityContextData<TSecurityContextIdent>> GetSecurityContexts();

    IEnumerable<SecurityContextData<TSecurityContextIdent>> GetSecurityContextsByIdents(IEnumerable<TSecurityContextIdent> securityContextIdents);

    IEnumerable<SecurityContextData<TSecurityContextIdent>> GetSecurityContextsWithMasterExpand(TSecurityContextIdent startSecurityEntityId);

    bool IsExists (TSecurityContextIdent securityContextId);
}
