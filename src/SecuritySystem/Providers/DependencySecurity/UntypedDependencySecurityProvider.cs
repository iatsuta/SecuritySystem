﻿using CommonFramework;
using SecuritySystem.SecurityAccessor;
using SecuritySystem.Services;

namespace SecuritySystem.Providers.DependencySecurity;

public class UntypedDependencySecurityProvider<TDomainObject, TBaseDomainObject> : ISecurityProvider<TDomainObject>
    where TDomainObject : IIdentityObject<Guid>
    where TBaseDomainObject : class, IIdentityObject<Guid>
{
    private readonly ISecurityProvider<TBaseDomainObject> baseSecurityProvider;

    private readonly IQueryableSource queryableSource;

    private readonly Lazy<HashSet<Guid>> lazyAvailableIdents;

    public UntypedDependencySecurityProvider(ISecurityProvider<TBaseDomainObject> baseSecurityProvider, IQueryableSource queryableSource)
    {
        this.baseSecurityProvider = baseSecurityProvider;
        this.queryableSource = queryableSource;

        this.lazyAvailableIdents = LazyHelper.Create(() => this.GetAvailableIdents().ToHashSet());
    }

    public IQueryable<TDomainObject> InjectFilter(IQueryable<TDomainObject> queryable)
    {
        var availableIdents = this.GetAvailableIdents();

        return queryable.Where(domainObj => availableIdents.Contains(domainObj.Id));
    }

    public AccessResult GetAccessResult(TDomainObject domainObject)
    {
        return this.baseSecurityProvider.GetAccessResult(this.GetBaseObject(domainObject)).TryOverrideDomainObject(domainObject);
    }

    public bool HasAccess(TDomainObject domainObject)
    {
        return this.lazyAvailableIdents.Value.Contains(domainObject.Id);
    }

    public SecurityAccessorData GetAccessorData(TDomainObject domainObject)
    {
        return this.baseSecurityProvider.GetAccessorData(this.GetBaseObject(domainObject));
    }

    private TBaseDomainObject GetBaseObject(TDomainObject domainObject)
    {
        return this.queryableSource
                   .GetQueryable<TBaseDomainObject>()
                   .SingleOrDefault(v => v.Id.Equals(domainObject.Id))
                   .FromMaybe(() => $"Object with id = '{domainObject.Id}' not found");
    }

    protected virtual IQueryable<Guid> GetAvailableIdents()
    {
        return this.queryableSource.GetQueryable<TBaseDomainObject>().Pipe(this.baseSecurityProvider.InjectFilter).Select(v => v.Id);
    }
}
