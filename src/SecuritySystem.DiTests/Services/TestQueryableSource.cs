using CommonFramework.GenericRepository;

namespace SecuritySystem.DiTests.Services;

public class TestQueryableSource : IQueryableSource
{
    public IQueryableSource BaseQueryableSource { get; set; } = Substitute.For<IQueryableSource>();

    public IQueryable<TDomainObject> GetQueryable<TDomainObject>()
        where TDomainObject : class
    {
        return this.BaseQueryableSource.GetQueryable<TDomainObject>();
    }
}