using CommonFramework.GenericRepository;

namespace SecuritySystem.DiTests.Services;

public class TestGenericRepository : IGenericRepository
{
	public Task SaveAsync<TDomainObject>(TDomainObject data, CancellationToken cancellationToken) where TDomainObject : class
	{
		throw new NotImplementedException();
	}

	public Task RemoveAsync<TDomainObject>(TDomainObject data, CancellationToken cancellationToken) where TDomainObject : class
	{
		throw new NotImplementedException();
	}
}