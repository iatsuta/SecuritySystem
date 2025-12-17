namespace SecuritySystem.Services;

public interface ISecurityRepository<TDomainObject>
	where TDomainObject : class
{
	Task<TDomainObject> GetObjectAsync(TypedSecurityIdentity securityIdentity, CancellationToken cancellationToken);
}