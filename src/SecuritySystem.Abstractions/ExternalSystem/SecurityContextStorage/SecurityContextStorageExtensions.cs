namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public static class SecurityContextStorageExtensions
{
    public static ITypedSecurityContextStorage<TIdent> GetTyped<TIdent>(this ISecurityContextStorage storage,
        Type securityContextType)
        where TIdent : notnull
    {
        return (ITypedSecurityContextStorage<TIdent>)storage.GetTyped(securityContextType);
    }
}