namespace SecuritySystem.Services;

public class SecurityRepository<TDomainObject> : ISecurityRepository<TDomainObject>
	where TDomainObject : class
{
	public Task<TDomainObject> GetObjectAsync(SecurityIdentity securityIdentity, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
