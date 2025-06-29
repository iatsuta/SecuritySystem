﻿using System.Linq.Expressions;
using CommonFramework;


namespace SecuritySystem.Builders.AccessorsBuilder;

public class OrFilterBuilder<TPermission, TDomainObject>(
    AccessorsFilterBuilderFactory<TPermission, TDomainObject> builderFactory,
    SecurityPath<TDomainObject>.OrSecurityPath securityPath,
    IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    : BinaryFilterBuilder<TPermission, TDomainObject, SecurityPath<TDomainObject>.OrSecurityPath>(
        builderFactory,
        securityPath,
        securityContextRestrictions)
{
    protected override Expression<Func<TArg, bool>> BuildOperation<TArg>(
        Expression<Func<TArg, bool>> arg1,
        Expression<Func<TArg, bool>> arg2) => arg1.BuildOr(arg2);
}
