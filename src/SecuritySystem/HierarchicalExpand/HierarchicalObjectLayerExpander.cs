//using CommonFramework;

//using SecuritySystem.ExpressionEvaluate;
//using SecuritySystem.Services;

//namespace SecuritySystem.HierarchicalExpand;

//public class HierarchicalObjectLayerExpander<TDomainObject, TIdent>(IQueryableSource queryableSource, HierarchicalInfo<TDomainObject> hierarchicalInfo)
//    : IHierarchicalObjectExpander<TIdent>
//    where TDomainObject : class, IIdentityObject<TIdent>
//    where TIdent : struct
//{
//    private readonly IQueryableSource queryableSource = queryableSource ?? throw new ArgumentNullException(nameof(queryableSource));

//    public IEnumerable<TIdent> Expand(IEnumerable<TIdent> idents, HierarchicalExpandType expandType)
//    {
//        return this.queryableSource.GetQueryable<TDomainObject>().Expand(idents, hierarchicalInfo, expandType);
//    }

//    public Dictionary<TIdent, TIdent> ExpandWithParents(IEnumerable<TIdent> idents, HierarchicalExpandType expandType)
//    {
//        return this.ExpandWithParentsImplementation(idents, expandType);
//    }

//    public Dictionary<TIdent, TIdent> ExpandWithParents(IQueryable<TIdent> idents, HierarchicalExpandType expandType)
//    {
//        return this.ExpandWithParentsImplementation(idents, expandType);
//    }

//    private Dictionary<TIdent, TIdent> ExpandWithParentsImplementation(IEnumerable<TIdent> idents, HierarchicalExpandType expandType)
//    {
//        var expandedIdents = this.Expand(idents, expandType).ToList();

//        var filteredQuery = this.queryableSource.GetQueryable<TDomainObject>().Where(domainObject => expandedIdents.Contains(domainObject.Id));

//        var resultSelector = ExpressionEvaluateHelper.InlineEvaluate(ee =>
//            ExpressionHelper.Create((TDomainObject domainObject) => new
//            {
//                domainObject.Id,

//                ParentId = (TIdent?)(ee.Evaluate(hierarchicalInfo.ParentPath, domainObject)!.Id)
//            }));

//        return filteredQuery.Select(resultSelector).ToDictionary(pair => pair.Id, pair => pair.ParentId.GetValueOrDefault());
//    }
//}