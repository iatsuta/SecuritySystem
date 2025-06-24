using SecuritySystem.PersistStorage;

namespace ExampleWebApp.Infrastructure.Services;

public class EfPersistStorage<TDomainObject>(TestDbContext dbContext) : IPersistStorage<TDomainObject>
    where TDomainObject : class
{
    public async Task SaveAsync(TDomainObject data, CancellationToken cancellationToken)
    {
        dbContext.Update(data);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}