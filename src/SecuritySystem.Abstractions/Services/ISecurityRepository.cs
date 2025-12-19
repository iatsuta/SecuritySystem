namespace SecuritySystem.Services;

public interface ISecurityRepository<TDomainObject>
	where TDomainObject : class
{
	Task<TDomainObject> GetObjectAsync(SecurityIdentity securityIdentity, CancellationToken cancellationToken);
}