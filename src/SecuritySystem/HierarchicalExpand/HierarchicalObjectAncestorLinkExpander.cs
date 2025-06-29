﻿using System.Linq.Expressions;

using CommonFramework;

using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.Services;

namespace SecuritySystem.HierarchicalExpand;

public class HierarchicalObjectAncestorLinkExpander<TDomainObject, TDirectedAncestorLink, TUndirectedAncestorLink, TIdent>(
    IQueryableSource queryableSource,
    HierarchicalInfo<TDomainObject, TDirectedAncestorLink, TUndirectedAncestorLink> hierarchicalInfo)
    : IHierarchicalObjectExpander<TIdent>, IHierarchicalObjectQueryableExpander<TIdent>

    where TDomainObject : IIdentityObject<TIdent>
    where TDirectedAncestorLink : class
    where TUndirectedAncestorLink : class
    where TIdent : notnull
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

            HierarchicalExpandType.Children => this.ExpandEnumerable(idents, hierarchicalInfo.DirectedAncestorLinkInfo.FromPath, hierarchicalInfo.DirectedAncestorLinkInfo.ToPath),

            HierarchicalExpandType.Parents => this.ExpandEnumerable(idents, hierarchicalInfo.DirectedAncestorLinkInfo.ToPath, hierarchicalInfo.DirectedAncestorLinkInfo.FromPath),

            HierarchicalExpandType.All => this.ExpandEnumerable(idents, hierarchicalInfo.UndirectedAncestorLinkInfo.FromPath, hierarchicalInfo.UndirectedAncestorLinkInfo.ToPath),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private IEnumerable<TIdent> ExpandEnumerable<TAncestorLink>(
        HashSet<TIdent> idents,
        Expression<Func<TAncestorLink, TDomainObject>> sourcePath,
        Expression<Func<TAncestorLink, TDomainObject>> targetPath)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = sourcePath.Select(domainObject => domainObject.Id);

        var toPathIdExpr = targetPath.Select(domainObject => domainObject.Id);

        var containsExpr = ExpressionEvaluateHelper.InlineEvaluate(ee =>
            ExpressionHelper.Create((TAncestorLink ancestorLink) => idents.Contains(ee.Evaluate(fromPathIdExpr, ancestorLink))));

        return ancestorLinkQueryable.Where(containsExpr).Select(toPathIdExpr);
    }

    public IQueryable<TIdent> ExpandQueryable(IQueryable<TIdent> idents, HierarchicalExpandType expandType)
    {
        return expandType switch
        {
            HierarchicalExpandType.None => idents,

            HierarchicalExpandType.Children => this.ExpandQueryable(idents, hierarchicalInfo.DirectedAncestorLinkInfo.FromPath, hierarchicalInfo.DirectedAncestorLinkInfo.ToPath),

            HierarchicalExpandType.Parents => this.ExpandQueryable(idents, hierarchicalInfo.DirectedAncestorLinkInfo.ToPath, hierarchicalInfo.DirectedAncestorLinkInfo.FromPath),

            HierarchicalExpandType.All => this.ExpandQueryable(idents, hierarchicalInfo.UndirectedAncestorLinkInfo.FromPath, hierarchicalInfo.UndirectedAncestorLinkInfo.ToPath),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private IQueryable<TIdent> ExpandQueryable<TAncestorLink>(
        IQueryable<TIdent> idents,
        Expression<Func<TAncestorLink, TDomainObject>> sourcePath,
        Expression<Func<TAncestorLink, TDomainObject>> targetPath)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = sourcePath.Select(domainObject => domainObject.Id);

        var toPathIdExpr = targetPath.Select(domainObject => domainObject.Id);

        var containsExpr = ExpressionEvaluateHelper.InlineEvaluate(ee =>
            ExpressionHelper.Create((TAncestorLink ancestorLink) => idents.Contains(ee.Evaluate(fromPathIdExpr, ancestorLink))));

        return ancestorLinkQueryable.Where(containsExpr).Select(toPathIdExpr);
    }

    public Expression<Func<IEnumerable<TIdent>, IEnumerable<TIdent>>> GetExpandExpression(HierarchicalExpandType expandType)
    {
        return expandType switch
        {
            HierarchicalExpandType.None => idents => idents,

            HierarchicalExpandType.Children => this.GetExpandExpression(hierarchicalInfo.DirectedAncestorLinkInfo.FromPath, hierarchicalInfo.DirectedAncestorLinkInfo.ToPath),

            HierarchicalExpandType.Parents => this.GetExpandExpression(hierarchicalInfo.DirectedAncestorLinkInfo.ToPath, hierarchicalInfo.DirectedAncestorLinkInfo.FromPath),

            HierarchicalExpandType.All => this.GetExpandExpression(hierarchicalInfo.UndirectedAncestorLinkInfo.FromPath, hierarchicalInfo.UndirectedAncestorLinkInfo.ToPath),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private Expression<Func<IEnumerable<TIdent>, IEnumerable<TIdent>>> GetExpandExpression<TAncestorLink>(
        Expression<Func<TAncestorLink, TDomainObject>> sourcePath,
        Expression<Func<TAncestorLink, TDomainObject>> targetPath)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = sourcePath.Select(domainObject => domainObject.Id);

        var toPathIdExpr = targetPath.Select(domainObject => domainObject.Id);

        return ExpressionEvaluateHelper.InlineEvaluate(ee =>

            ExpressionHelper.Create<IEnumerable<TIdent>, IEnumerable<TIdent>>(idents =>

                ancestorLinkQueryable.Where(ancestorLink => idents.Contains(ee.Evaluate(fromPathIdExpr, ancestorLink))).Select(toPathIdExpr)));
    }

    public Expression<Func<TIdent, IEnumerable<TIdent>>>? TryGetSingleExpandExpression(HierarchicalExpandType expandType)
    {
        return expandType switch
        {
            HierarchicalExpandType.None => null,

            HierarchicalExpandType.Children => this.GetSingleExpandExpression(hierarchicalInfo.DirectedAncestorLinkInfo.FromPath, hierarchicalInfo.DirectedAncestorLinkInfo.ToPath),

            HierarchicalExpandType.Parents => this.GetSingleExpandExpression(hierarchicalInfo.DirectedAncestorLinkInfo.ToPath, hierarchicalInfo.DirectedAncestorLinkInfo.FromPath),

            HierarchicalExpandType.All => this.GetSingleExpandExpression(hierarchicalInfo.UndirectedAncestorLinkInfo.FromPath, hierarchicalInfo.UndirectedAncestorLinkInfo.ToPath),

            _ => throw new ArgumentOutOfRangeException(nameof(expandType))
        };
    }

    private Expression<Func<TIdent, IEnumerable<TIdent>>> GetSingleExpandExpression<TAncestorLink>(
        Expression<Func<TAncestorLink, TDomainObject>> sourcePath,
        Expression<Func<TAncestorLink, TDomainObject>> targetPath)
        where TAncestorLink : class
    {
        var ancestorLinkQueryable = queryableSource.GetQueryable<TAncestorLink>();

        var fromPathIdExpr = sourcePath.Select(domainObject => domainObject.Id);

        var toPathIdExpr = targetPath.Select(domainObject => domainObject.Id);

        var eqIdentsExpr = ExpressionHelper.GetEquality<TIdent>();

        return ExpressionEvaluateHelper.InlineEvaluate(ee =>

            ExpressionHelper.Create<TIdent, IEnumerable<TIdent>>(ident =>

                ancestorLinkQueryable.Where(ancestorLink => ee.Evaluate(eqIdentsExpr, ident, ee.Evaluate(fromPathIdExpr, ancestorLink))).Select(toPathIdExpr)));
    }
}