using SecuritySystem.Services;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class LocalStorage<TSecurityContext, TIdent>(IIdentityInfoSource identityInfoSource)
    where TSecurityContext : ISecurityContext
    where TIdent : notnull
{
    private readonly IdentityInfo<TSecurityContext, TIdent> identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TIdent>();

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