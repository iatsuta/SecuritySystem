using CommonFramework.GenericRepository;

using GenericQueryable;

namespace SecuritySystem.Services;

public class SecurityRepository<TDomainObject>(IQueryableSource queryableSource, ISecurityIdentityFilterFactory<TDomainObject> filterFactory)
	: ISecurityRepository<TDomainObject>
	where TDomainObject : class
{
	public async Task<TDomainObject> GetObjectAsync(SecurityIdentity securityIdentity, CancellationToken cancellationToken)
	{
		var result = await queryableSource.GetQueryable<TDomainObject>().Where(filterFactory.CreateFilter(securityIdentity))
			.GenericSingleOrDefaultAsync(cancellationToken);

		return result ?? throw new ArgumentOutOfRangeException(nameof(securityIdentity),
			$"{typeof(TDomainObject).Name} with {nameof(securityIdentity)} '{securityIdentity}' not found");
	}
}