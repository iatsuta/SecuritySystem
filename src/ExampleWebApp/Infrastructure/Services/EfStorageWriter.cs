using SecuritySystem.PersistStorage;

namespace ExampleWebApp.Infrastructure.Services;

public class EfStorageWriter(TestDbContext dbContext) : IStorageWriter
{
    public async Task SaveAsync<TDomainObject>(TDomainObject data, CancellationToken cancellationToken)
        where TDomainObject : class
    {
        dbContext.Update(data);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}