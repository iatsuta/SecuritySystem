using CommonFramework.IdentitySource;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class LocalStorage<TSecurityContext, TSecurityContextIdent>(IIdentityInfoSource identityInfoSource)
	where TSecurityContext : ISecurityContext
	where TSecurityContextIdent : notnull
{
	private readonly IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo =
		identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextIdent>();

	private readonly HashSet<TSecurityContext> items = [];

	public bool IsExists(TSecurityContextIdent securityEntityId)
	{
		return this.items.Select(identityInfo.Id.Getter).Contains(securityEntityId);
	}

	public bool Register(TSecurityContext securityContext)
	{
		return this.items.Add(securityContext);
	}
}