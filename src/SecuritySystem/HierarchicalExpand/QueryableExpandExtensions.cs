//using CommonFramework;

//using SecuritySystem.ExpressionEvaluate;

//namespace SecuritySystem.HierarchicalExpand;

//public static class QueryableExpandExtensions
//{
//    public static IEnumerable<TIdent> Expand<TDomainObject, TIdent>(this IQueryable<TDomainObject> queryable, IEnumerable<TIdent> idents, HierarchicalInfo<TDomainObject> hierarchicalInfo, HierarchicalExpandType expandType)
//        where TDomainObject : class, IIdentityObject<TIdent>
//    {
//        if (expandType == HierarchicalExpandType.None)
//        {
//            return idents.ToHashSet();
//        }
//        else
//        {
//            return queryable.ExpandMany(idents, hierarchicalInfo, expandType).SelectMany(v => v).ToHashSet();
//        }
//    }

//    private static IEnumerable<HashSet<TIdent>> ExpandMany<TDomainObject, TIdent>(this IQueryable<TDomainObject> queryable, IEnumerable<TIdent> idents, HierarchicalInfo<TDomainObject> hierarchicalInfo, HierarchicalExpandType expandType)
//        where TDomainObject : class, IIdentityObject<TIdent>
//    {
//        var cachedIdents = idents.ToHashSet();

//        yield return cachedIdents;

//        yield return expandType.HasFlag(HierarchicalExpandType.Parents) ? queryable.ExpandMasters(cachedIdents, hierarchicalInfo) : [];

//        yield return expandType.HasFlag(HierarchicalExpandType.Children) ? queryable.ExpandChildren(cachedIdents, hierarchicalInfo) : [];
//    }

//    private static HashSet<TIdent> ExpandMasters<TDomainObject, TIdent>(this IQueryable<TDomainObject> queryable, IEnumerable<TIdent> baseIdents, HierarchicalInfo<TDomainObject> hierarchicalInfo)
//        where TDomainObject : class, IIdentityObject<TIdent>
//    {
//        var currentIdents = baseIdents.ToHashSet();

//        var getParentIdExpr = ExpressionEvaluateHelper.InlineEvaluate<Func<TDomainObject, TIdent?>>(ee =>
//            ExpressionHelper.Create((TDomainObject domainObject) => (TIdent?)ee.Evaluate(hierarchicalInfo.ParentPath, domainObject)!.Id));

//        for (var roots = currentIdents.ToHashSet(); roots.Any(); currentIdents.AddRange(roots))
//        {
//            var newRoots =

//                from domainObjectId in queryable.Where(domainObject => roots.Contains(domainObject.Id)).Select(getParentIdExpr)

//                where domainObjectId != null && !currentIdents.Contains(domainObjectId)

//                select domainObjectId;

//            roots = newRoots.ToHashSet();
//        }

//        return currentIdents;
//    }

//    private static HashSet<TIdent> ExpandChildren<TDomainObject, TIdent>(this IQueryable<TDomainObject> queryable, IEnumerable<TIdent> baseIdents, HierarchicalInfo<TDomainObject> hierarchicalInfo)
//        where TDomainObject : class, IIdentityObject<TIdent>
//    {
//        var currentIdents = baseIdents.ToHashSet();

//        for (var children = new HashSet<TIdent>(currentIdents); children.Any(); currentIdents.AddRange(children))
//        {
//            children = queryable.Where(v =>
//                !currentIdents.Contains(v.Id) && (v.Parent != null && children.Contains(v.Parent.Id))).Select(v => v.Id).ToHashSet();
//        }

//        return currentIdents;
//    }
//}