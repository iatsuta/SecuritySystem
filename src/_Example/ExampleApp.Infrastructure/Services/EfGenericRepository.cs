using SecuritySystem.Services;

namespace ExampleApp.Infrastructure.Services;

public class EfGenericRepository(TestDbContext dbContext) : IGenericRepository
{
    public async Task SaveAsync<TDomainObject>(TDomainObject data, CancellationToken cancellationToken)
        where TDomainObject : class
    {
        dbContext.Update(data);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync<TDomainObject>(TDomainObject data, CancellationToken cancellationToken)
        where TDomainObject : class
    {
        dbContext.Remove(data);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}