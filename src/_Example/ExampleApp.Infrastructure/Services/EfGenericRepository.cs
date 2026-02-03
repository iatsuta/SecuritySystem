using CommonFramework.GenericRepository;

using Microsoft.EntityFrameworkCore;

namespace ExampleApp.Infrastructure.Services;

public class EfGenericRepository(TestDbContext dbContext) : IGenericRepository
{
    public async Task SaveAsync<TDomainObject>(TDomainObject domainObject, CancellationToken cancellationToken)
        where TDomainObject : class
    {
        var state = dbContext.Entry(domainObject).State;

        if (state == EntityState.Detached)
        {
            await dbContext.AddAsync(domainObject, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync<TDomainObject>(TDomainObject domainObject, CancellationToken cancellationToken)
        where TDomainObject : class
    {
        dbContext.Remove(domainObject);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}