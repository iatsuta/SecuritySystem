using System.Collections;
using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;

using SecuritySystem.Services;

namespace SecuritySystem.HierarchicalExpand;

public class HierarchicalObjectAncestorLinkExpander<TDomainObject, TDirectedAncestorLink, TUndirectedAncestorLink, TIdent>(
    IQueryableSource queryableSource,
    FullAncestorLinkInfo<TDomainObject, TDirectedAncestorLink, TUndirectedAncestorLink> fullAncestorLinkInfo,
    HierarchicalInfo<TDomainObject> hierarchicalInfo,
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

            HierarchicalExpandType.Children => this.ExpandEnumerable(idents, fullAncestorLinkInfo.Directed),

            HierarchicalExpandType.Parents => this.ExpandEnumerable(idents, fullAncestorLinkInfo.Directed.Reverse()),

            HierarchicalExpandType.All => this.ExpandEnumerable(idents, fullAncestorLinkInfo.Undirected),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private IQueryable<TIdent> ExpandEnumerable<TAncestorLink>(
        HashSet<TIdent> idents,
        AncestorLinkInfo<TDomainObject, TAncestorLink> ancestorLinkInfo)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = ancestorLinkInfo.From.Path.Select(identityInfo.Id.Path);

        var toPathIdExpr = ancestorLinkInfo.To.Path.Select(identityInfo.Id.Path);

        var containsExpr = ExpressionEvaluateHelper.InlineEvaluate(ee =>
            ExpressionHelper.Create((TAncestorLink ancestorLink) => idents.Contains(ee.Evaluate(fromPathIdExpr, ancestorLink))));

        return ancestorLinkQueryable.Where(containsExpr).Select(toPathIdExpr);
    }

    public IQueryable<TIdent> ExpandQueryable(IQueryable<TIdent> idents, HierarchicalExpandType expandType)
    {
        return expandType switch
        {
            HierarchicalExpandType.None => idents,

            HierarchicalExpandType.Children => this.ExpandQueryable(idents, fullAncestorLinkInfo.Directed),

            HierarchicalExpandType.Parents => this.ExpandQueryable(idents, fullAncestorLinkInfo.Directed.Reverse()),

            HierarchicalExpandType.All => this.ExpandQueryable(idents, fullAncestorLinkInfo.Undirected),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private IQueryable<TIdent> ExpandQueryable<TAncestorLink>(
        IQueryable<TIdent> idents,
        AncestorLinkInfo<TDomainObject, TAncestorLink> ancestorLinkInfo)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = ancestorLinkInfo.From.Path.Select(identityInfo.Id.Path);

        var toPathIdExpr = ancestorLinkInfo.To.Path.Select(identityInfo.Id.Path);

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

            HierarchicalExpandType.Children => this.GetExpandExpression(fullAncestorLinkInfo.Directed),

            HierarchicalExpandType.Parents => this.GetExpandExpression(
                fullAncestorLinkInfo.Directed.Reverse()),

            HierarchicalExpandType.All => this.GetExpandExpression(fullAncestorLinkInfo.Undirected),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private Expression<Func<IEnumerable<TIdent>, IEnumerable<TIdent>>> GetExpandExpression<TAncestorLink>(
        AncestorLinkInfo<TDomainObject, TAncestorLink> ancestorLinkInfo)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = ancestorLinkInfo.From.Path.Select(identityInfo.Id.Path);

        var toPathIdExpr = ancestorLinkInfo.To.Path.Select(identityInfo.Id.Path);

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
                this.GetSingleExpandExpression(fullAncestorLinkInfo.Directed),

            HierarchicalExpandType.Parents => this.GetSingleExpandExpression(fullAncestorLinkInfo.Directed
                .Reverse()),

            HierarchicalExpandType.All => this.GetSingleExpandExpression(fullAncestorLinkInfo.Undirected),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private Expression<Func<TIdent, IEnumerable<TIdent>>> GetSingleExpandExpression<TAncestorLink>(
        AncestorLinkInfo<TDomainObject, TAncestorLink> ancestorLinkInfo)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = ancestorLinkInfo.From.Path.Select(identityInfo.Id.Path);

        var toPathIdExpr = ancestorLinkInfo.To.Path.Select(identityInfo.Id.Path);

        var eqIdentsExpr = ExpressionHelper.GetEquality<TIdent>();

        return ExpressionEvaluateHelper.InlineEvaluate(ee =>

            ExpressionHelper.Create<TIdent, IEnumerable<TIdent>>(ident =>

                ancestorLinkQueryable
                    .Where(ancestorLink => ee.Evaluate(eqIdentsExpr, ident, ee.Evaluate(fromPathIdExpr, ancestorLink)))
                    .Select(toPathIdExpr)));
    }

    public Dictionary<TIdent, TIdent> ExpandWithParents(IEnumerable<TIdent> idents, HierarchicalExpandType expandType)
    {
        return this.ExpandWithParentsImplementation(idents.ToHashSet(), expandType);
    }

    public Dictionary<TIdent, TIdent> ExpandWithParents(IQueryable<TIdent> idents, HierarchicalExpandType expandType)
    {
        return this.ExpandWithParentsImplementation(idents, expandType);
    }

    private Dictionary<TIdent, TIdent> ExpandWithParentsImplementation(IEnumerable<TIdent> idents, HierarchicalExpandType expandType)
    {
        return this
            .ExpandDomainObject(idents, expandType)
            .Select(ExpressionEvaluateHelper.InlineEvaluate(ee =>

                ExpressionHelper.Create((TDomainObject domainObject) => new
                {
                    Id = ee.Evaluate(identityInfo.Id.Path, domainObject),
                    ParentId = ee.Evaluate(hierarchicalInfo.ParentPath, domainObject) == null
                        ? default
                        : ee.Evaluate(identityInfo.Id.Path!, ee.Evaluate(hierarchicalInfo.ParentPath, domainObject))
                })))
            .Distinct()
            .ToDictionary(pair => pair.Id, pair => pair.ParentId!);
    }

    private IQueryable<TDomainObject> ExpandDomainObject(IEnumerable<TIdent> idents, HierarchicalExpandType expandType)
    {
        switch (expandType)
        {
            case HierarchicalExpandType.None:
            {
                var filter = identityInfo.Id.Path.Select(domainObjectId => idents.Contains(domainObjectId));

                return queryableSource.GetQueryable<TDomainObject>().Where(filter);
            }

            case HierarchicalExpandType.Children:
                return this.ExpandDomainObject(idents, fullAncestorLinkInfo.Directed);

            case HierarchicalExpandType.Parents:
                return this.ExpandDomainObject(idents, fullAncestorLinkInfo.Directed.Reverse());

            case HierarchicalExpandType.All:
                return this.ExpandDomainObject(idents, fullAncestorLinkInfo.Undirected);

            default:
                throw new ArgumentOutOfRangeException(nameof(expandType));
        }
    }

    private IQueryable<TDomainObject> ExpandDomainObject<TAncestorLink>(
        IEnumerable<TIdent> idents,
        AncestorLinkInfo<TDomainObject, TAncestorLink> ancestorLinkInfo)
        where TAncestorLink : class
    {
        var idPath = ancestorLinkInfo.From.Path.Select(identityInfo.Id.Path);

        var filter = idPath.Select(domainObjectId => idents.Contains(domainObjectId));

        return queryableSource.GetQueryable<TAncestorLink>().Where(filter).Select(ancestorLinkInfo.To.Path);
    }

    public IEnumerable Expand(IEnumerable idents, HierarchicalExpandType expandType)
    {
        return this.Expand((IEnumerable<TIdent>)idents, expandType);
    }
}