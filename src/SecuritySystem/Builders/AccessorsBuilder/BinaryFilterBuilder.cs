﻿using SecuritySystem.HierarchicalExpand;
using System.Linq.Expressions;


namespace SecuritySystem.Builders.AccessorsBuilder;

public abstract class BinaryFilterBuilder<TPermission, TDomainObject, TSecurityPath>(
    AccessorsFilterBuilderFactory<TPermission, TDomainObject> builderFactory,
    TSecurityPath securityPath,
    IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    : AccessorsFilterBuilder<TPermission, TDomainObject>
    where TSecurityPath : SecurityPath<TDomainObject>.BinarySecurityPath
{
    private AccessorsFilterBuilder<TPermission, TDomainObject> LeftBuilder { get; } = builderFactory.CreateBuilder(securityPath.Left, securityContextRestrictions);

    private AccessorsFilterBuilder<TPermission, TDomainObject> RightBuilder { get; } = builderFactory.CreateBuilder(securityPath.Right, securityContextRestrictions);

    protected abstract Expression<Func<TArg, bool>> BuildOperation<TArg>(
        Expression<Func<TArg, bool>> arg1,
        Expression<Func<TArg, bool>> arg2);

    public override Expression<Func<TPermission, bool>> GetAccessorsFilter(TDomainObject domainObject, HierarchicalExpandType expandType)
    {
        var leftFilter = this.LeftBuilder.GetAccessorsFilter(domainObject, expandType);

        var rightFilter = this.RightBuilder.GetAccessorsFilter(domainObject, expandType);

        return this.BuildOperation(leftFilter, rightFilter);
    }
}
