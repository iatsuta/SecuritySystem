namespace SecuritySystem.Services;

public interface IQueryableSource
{
    IQueryable<TDomainObject> GetQueryable<TDomainObject>()
        where TDomainObject : class;
}