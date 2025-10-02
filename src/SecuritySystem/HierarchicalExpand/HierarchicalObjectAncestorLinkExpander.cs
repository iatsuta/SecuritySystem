using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using SecuritySystem.Services;

namespace SecuritySystem.HierarchicalExpand;

public class HierarchicalObjectAncestorLinkExpander<TDomainObject, TDirectedAncestorLink, TUndirectedAncestorLink, TIdent>(
    IQueryableSource queryableSource,
    HierarchicalInfo<TDomainObject, TDirectedAncestorLink, TUndirectedAncestorLink> hierarchicalInfo,
    IdentityInfo<TDomainObject, TIdent> identityInfo)
    : IHierarchicalObjectExpander<TIdent>

    where TDirectedAncestorLink : class
    where TUndirectedAncestorLink : class
    where TIdent : notnull
    where TDomainObject : class
{
    public IEnumerable<TIdent> Expand(IEnumerable<TIdent> idents, HierarchicalExpandType expandType)
    {
        if (idents is IQueryable<TIdent> queryable)
        {
            return this.ExpandQueryable(queryable, expandType);
        }
        else
        {
            return this.ExpandEnumerable(idents, expandType);
        }
    }

    public IEnumerable<TIdent> ExpandEnumerable(IEnumerable<TIdent> baseIdents, HierarchicalExpandType expandType)
    {
        var idents = baseIdents.ToHashSet();

        return expandType switch
        {
            HierarchicalExpandType.None => idents,

            HierarchicalExpandType.Children => this.ExpandEnumerable(idents, hierarchicalInfo.DirectedAncestorLinkInfo),

            HierarchicalExpandType.Parents => this.ExpandEnumerable(idents, hierarchicalInfo.DirectedAncestorLinkInfo.Reverse()),

            HierarchicalExpandType.All => this.ExpandEnumerable(idents, hierarchicalInfo.UndirectedAncestorLinkInfo),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private IEnumerable<TIdent> ExpandEnumerable<TAncestorLink>(
        HashSet<TIdent> idents,
        AncestorLinkInfo<TDomainObject, TAncestorLink> ancestorLinkInfo)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = ancestorLinkInfo.FromPath.Select(identityInfo.IdPath);

        var toPathIdExpr = ancestorLinkInfo.ToPath.Select(identityInfo.IdPath);

        var containsExpr = ExpressionEvaluateHelper.InlineEvaluate(ee =>
            ExpressionHelper.Create((TAncestorLink ancestorLink) => idents.Contains(ee.Evaluate(fromPathIdExpr, ancestorLink))));

        return ancestorLinkQueryable.Where(containsExpr).Select(toPathIdExpr);
    }

    public IQueryable<TIdent> ExpandQueryable(IQueryable<TIdent> idents, HierarchicalExpandType expandType)
    {
        return expandType switch
        {
            HierarchicalExpandType.None => idents,

            HierarchicalExpandType.Children => this.ExpandQueryable(idents, hierarchicalInfo.DirectedAncestorLinkInfo),

            HierarchicalExpandType.Parents => this.ExpandQueryable(idents, hierarchicalInfo.DirectedAncestorLinkInfo.Reverse()),

            HierarchicalExpandType.All => this.ExpandQueryable(idents, hierarchicalInfo.UndirectedAncestorLinkInfo),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private IQueryable<TIdent> ExpandQueryable<TAncestorLink>(
        IQueryable<TIdent> idents,
        AncestorLinkInfo<TDomainObject, TAncestorLink> ancestorLinkInfo)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = ancestorLinkInfo.FromPath.Select(identityInfo.IdPath);

        var toPathIdExpr = ancestorLinkInfo.ToPath.Select(identityInfo.IdPath);

        var containsExpr = ExpressionEvaluateHelper.InlineEvaluate(ee =>
            ExpressionHelper.Create((TAncestorLink ancestorLink) =>
                idents.Contains(ee.Evaluate(fromPathIdExpr, ancestorLink))));

        return ancestorLinkQueryable.Where(containsExpr).Select(toPathIdExpr);
    }

    public Expression<Func<IEnumerable<TIdent>, IEnumerable<TIdent>>> GetExpandExpression(
        HierarchicalExpandType expandType)
    {
        return expandType switch
        {
            HierarchicalExpandType.None => idents => idents,

            HierarchicalExpandType.Children => this.GetExpandExpression(hierarchicalInfo.DirectedAncestorLinkInfo),

            HierarchicalExpandType.Parents => this.GetExpandExpression(
                hierarchicalInfo.DirectedAncestorLinkInfo.Reverse()),

            HierarchicalExpandType.All => this.GetExpandExpression(hierarchicalInfo.UndirectedAncestorLinkInfo),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private Expression<Func<IEnumerable<TIdent>, IEnumerable<TIdent>>> GetExpandExpression<TAncestorLink>(
        AncestorLinkInfo<TDomainObject, TAncestorLink> ancestorLinkInfo)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = ancestorLinkInfo.FromPath.Select(identityInfo.IdPath);

        var toPathIdExpr = ancestorLinkInfo.ToPath.Select(identityInfo.IdPath);

        return ExpressionEvaluateHelper.InlineEvaluate(ee =>

            ExpressionHelper.Create<IEnumerable<TIdent>, IEnumerable<TIdent>>(idents =>

                ancestorLinkQueryable.Where(ancestorLink => idents.Contains(ee.Evaluate(fromPathIdExpr, ancestorLink)))
                    .Select(toPathIdExpr)));
    }

    public Expression<Func<TIdent, IEnumerable<TIdent>>>? TryGetSingleExpandExpression(
        HierarchicalExpandType expandType)
    {
        return expandType switch
        {
            HierarchicalExpandType.None => null,

            HierarchicalExpandType.Children =>
                this.GetSingleExpandExpression(hierarchicalInfo.DirectedAncestorLinkInfo),

            HierarchicalExpandType.Parents => this.GetSingleExpandExpression(hierarchicalInfo.DirectedAncestorLinkInfo
                .Reverse()),

            HierarchicalExpandType.All => this.GetSingleExpandExpression(hierarchicalInfo.UndirectedAncestorLinkInfo),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private Expression<Func<TIdent, IEnumerable<TIdent>>> GetSingleExpandExpression<TAncestorLink>(
        AncestorLinkInfo<TDomainObject, TAncestorLink> ancestorLinkInfo)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = ancestorLinkInfo.FromPath.Select(identityInfo.IdPath);

        var toPathIdExpr = ancestorLinkInfo.ToPath.Select(identityInfo.IdPath);

        var eqIdentsExpr = ExpressionHelper.GetEquality<TIdent>();

        return ExpressionEvaluateHelper.InlineEvaluate(ee =>

            ExpressionHelper.Create<TIdent, IEnumerable<TIdent>>(ident =>

                ancestorLinkQueryable
                    .Where(ancestorLink => ee.Evaluate(eqIdentsExpr, ident, ee.Evaluate(fromPathIdExpr, ancestorLink)))
                    .Select(toPathIdExpr)));
    }

    public Dictionary<TIdent, TIdent?> ExpandWithParents(IEnumerable<TIdent> idents, HierarchicalExpandType expandType)
    {
        return this.ExpandWithParentsImplementation(idents.ToHashSet(), expandType);
    }

    public Dictionary<TIdent, TIdent?> ExpandWithParents(IQueryable<TIdent> idents, HierarchicalExpandType expandType)
    {
        return this.ExpandWithParentsImplementation(idents, expandType);
    }

    private record struct WithMasterData(TIdent Id, TIdent? ParentId);

    private Dictionary<TIdent, TIdent?> ExpandWithParentsImplementation(IEnumerable<TIdent> idents,
        HierarchicalExpandType expandType)
    {
        var selector =

            ExpressionEvaluateHelper.InlineEvaluate<Func<TDomainObject, WithMasterData>>(ee =>

                domainObject => new WithMasterData
                (
                    ee.Evaluate(identityInfo.IdPath, domainObject),

                    (TIdent?)ee.Evaluate(identityInfo.IdPath,
                        ee.Evaluate(hierarchicalInfo.ParentPath, domainObject)!)));


        return this
            .ExpandDomainObject(idents, expandType)
            .Select(selector)
            .Distinct()
            .ToDictionary(pair => pair.Id, pair => pair.ParentId);
    }

    private IQueryable<TDomainObject> ExpandDomainObject(
        IEnumerable<TIdent> idents,
        HierarchicalExpandType expandType)
    {
        switch (expandType)
        {
            case HierarchicalExpandType.None:
            {
                var filter = identityInfo.IdPath.Select(domainObjectId => idents.Contains(domainObjectId));

                return queryableSource.GetQueryable<TDomainObject>().Where(filter);
            }

            case HierarchicalExpandType.Children:

                return this.ExpandDomainObject(idents, hierarchicalInfo.DirectedAncestorLinkInfo);

            case HierarchicalExpandType.Parents:
                return this.ExpandDomainObject(idents, hierarchicalInfo.DirectedAncestorLinkInfo.Reverse());

            case HierarchicalExpandType.All:
                return this.ExpandDomainObject(idents, hierarchicalInfo.UndirectedAncestorLinkInfo);

            default:
                throw new ArgumentOutOfRangeException(nameof(expandType));
        }
    }

    private IQueryable<TDomainObject> ExpandDomainObject<TAncestorLink>(
        IEnumerable<TIdent> idents,
        AncestorLinkInfo<TDomainObject, TAncestorLink> ancestorLinkInfo) where TAncestorLink : class
    {
        var idPath = ancestorLinkInfo.FromPath.Select(identityInfo.IdPath);

        var filter = idPath.Select(domainObjectId => idents.Contains(domainObjectId));

        return queryableSource.GetQueryable<TAncestorLink>().Where(filter).Select(ancestorLinkInfo.ToPath);
    }

    public Array Expand(Array idents, HierarchicalExpandType expandType)
    {
        return this.Expand(idents.Cast<TIdent>(), expandType).ToArray();
    }
}