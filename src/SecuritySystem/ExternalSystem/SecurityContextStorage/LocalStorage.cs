namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class LocalStorage<TSecurityContext, TIdent>(IdentityInfo<TSecurityContext, TIdent> identityInfo)
    where TSecurityContext : ISecurityContext
    where TIdent : notnull
{
    private readonly HashSet<TSecurityContext> items = [];

    public bool IsExists(TIdent securityEntityId)
    {
        return this.items.Select(identityInfo.IdFunc).Contains(securityEntityId);
    }

    public bool Register(TSecurityContext securityContext)
    {
        return this.items.Add(securityContext);
    }
}