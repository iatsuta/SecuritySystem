using CommonFramework;
using CommonFramework.ExpressionEvaluate;

using SecuritySystem.SecurityAccessor;
using SecuritySystem.Services;

namespace SecuritySystem.Providers.DependencySecurity;

public class UntypedDependencySecurityProvider<TDomainObject, TBaseDomainObject, TIdent> : ISecurityProvider<TDomainObject>
    where TBaseDomainObject : class
    where TIdent : notnull
{
    private readonly ISecurityProvider<TBaseDomainObject> baseSecurityProvider;

    private readonly IQueryableSource queryableSource;

    private readonly IdentityInfo<TDomainObject, TIdent> domainIdentityInfo;

    private readonly IdentityInfo<TBaseDomainObject, TIdent> baseDomainIdentityInfo;

    private readonly Lazy<HashSet<TIdent>> lazyAvailableIdents;

    public UntypedDependencySecurityProvider(
        IQueryableSource queryableSource,
        ISecurityProvider<TBaseDomainObject> baseSecurityProvider,
        IdentityInfo<TDomainObject, TIdent> domainIdentityInfo,
        IdentityInfo<TBaseDomainObject, TIdent> baseDomainIdentityInfo)
    {
        this.baseSecurityProvider = baseSecurityProvider;
        this.queryableSource = queryableSource;
        this.domainIdentityInfo = domainIdentityInfo;
        this.baseDomainIdentityInfo = baseDomainIdentityInfo;

        this.lazyAvailableIdents = LazyHelper.Create(() => this.GetAvailableIdents().ToHashSet());
    }

    public IQueryable<TDomainObject> InjectFilter(IQueryable<TDomainObject> queryable)
    {
        var availableIdents = this.GetAvailableIdents();

        var filterExpr = ExpressionEvaluateHelper.InlineEvaluate<Func<TDomainObject, bool>>(ee =>

            domainObject => availableIdents.Contains(ee.Evaluate(domainIdentityInfo.Id.Path, domainObject)));

        return queryable.Where(filterExpr);
    }

    public AccessResult GetAccessResult(TDomainObject domainObject)
    {
        return this.baseSecurityProvider.GetAccessResult(this.GetBaseObject(domainObject)).TryOverrideDomainObject(domainObject);
    }

    public bool HasAccess(TDomainObject domainObject)
    {
        return this.lazyAvailableIdents.Value.Contains(domainIdentityInfo.Id.Getter(domainObject));
    }

    public SecurityAccessorData GetAccessorData(TDomainObject domainObject)
    {
        return this.baseSecurityProvider.GetAccessorData(this.GetBaseObject(domainObject));
    }

    private TBaseDomainObject GetBaseObject(TDomainObject domainObject)
    {
        var id = domainIdentityInfo.Id.Getter(domainObject);

        var eqIdExp = ExpressionHelper.GetEquality<TIdent>();

        var filterExpr = ExpressionEvaluateHelper.InlineEvaluate<Func<TBaseDomainObject, bool>>(ee =>

            baseDomainObject => ee.Evaluate(eqIdExp, ee.Evaluate(baseDomainIdentityInfo.Id.Path, baseDomainObject), id));

        return this.queryableSource
                   .GetQueryable<TBaseDomainObject>()
                   .SingleOrDefault(filterExpr)
                   .FromMaybe(() => $"{typeof(TBaseDomainObject).Name} with id = '{id}' not found");
    }

    protected virtual IQueryable<TIdent> GetAvailableIdents()
    {
        return this.queryableSource.GetQueryable<TBaseDomainObject>().Pipe(this.baseSecurityProvider.InjectFilter).Select(baseDomainIdentityInfo.Id.Path);
    }
}
