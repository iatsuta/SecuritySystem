using ExampleApp.Application;

using SecuritySystem.AccessDenied;
using SecuritySystem.Providers;

namespace ExampleApp.Infrastructure.Services;

public class EfRepository<TDomainObject>(
    TestDbContext dbContext,
    ISecurityProvider<TDomainObject> securityProvider,
    IAccessDeniedExceptionService accessDeniedExceptionService) : IRepository<TDomainObject>
    where TDomainObject : class
{
    public async Task UpdateAsync(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        this.CheckAccess(domainObject);

        dbContext.Update(domainObject);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task InsertAsync(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        this.CheckAccess(domainObject);

        dbContext.Add(domainObject);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        this.CheckAccess(domainObject);

        dbContext.Remove(domainObject);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void CheckAccess(TDomainObject domainObject)
    {
        securityProvider.CheckAccess(domainObject, accessDeniedExceptionService);
    }

    public IQueryable<TDomainObject> GetQueryable()
    {
        return securityProvider.InjectFilter(dbContext.Set<TDomainObject>());
    }
}