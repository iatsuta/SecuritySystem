﻿using SecuritySystem.HierarchicalExpand;
using System.Linq.Expressions;

namespace SecuritySystem.Builders.QueryBuilder;

public abstract class BinaryFilterBuilder<TPermission, TDomainObject, TSecurityPath>(
    SecurityFilterBuilderFactory<TPermission, TDomainObject> builderFactory,
    TSecurityPath securityPath,
    IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    : SecurityFilterBuilder<TPermission, TDomainObject>
    where TSecurityPath : SecurityPath<TDomainObject>.BinarySecurityPath
{
    private SecurityFilterBuilder<TPermission, TDomainObject> LeftBuilder { get; } = builderFactory.CreateBuilder(securityPath.Left, securityContextRestrictions);

    private SecurityFilterBuilder<TPermission, TDomainObject> RightBuilder { get; } = builderFactory.CreateBuilder(securityPath.Right, securityContextRestrictions);

    protected abstract Expression<Func<TArg1, TArg2, bool>> BuildOperation<TArg1, TArg2>(
        Expression<Func<TArg1, TArg2, bool>> arg1,
        Expression<Func<TArg1, TArg2, bool>> arg2);

    public override Expression<Func<TDomainObject, TPermission, bool>> GetSecurityFilterExpression(HierarchicalExpandType expandType)
    {
        var leftFilter = this.LeftBuilder.GetSecurityFilterExpression(expandType);
        var rightFilter = this.RightBuilder.GetSecurityFilterExpression(expandType);

        return this.BuildOperation(leftFilter, rightFilter);
    }
}
