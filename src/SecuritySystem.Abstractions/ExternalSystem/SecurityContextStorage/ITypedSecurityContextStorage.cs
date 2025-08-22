namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public interface ITypedSecurityContextStorage;

public interface ITypedSecurityContextStorage<TIdent> : ITypedSecurityContextStorage
{
    IEnumerable<SecurityContextData<TIdent>> GetSecurityContexts();

    IEnumerable<SecurityContextData<TIdent>> GetSecurityContextsByIdents(IEnumerable<TIdent> securityContextIdents);

    bool IsExists (TIdent securityContextId);
}
