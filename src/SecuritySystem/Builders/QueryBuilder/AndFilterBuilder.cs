using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.Builders.QueryBuilder;

public class AndFilterBuilder<TPermission, TDomainObject>(
    SecurityFilterBuilderFactory<TPermission, TDomainObject> builderFactory,
    SecurityPath<TDomainObject>.AndSecurityPath securityPath,
    IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    : BinaryFilterBuilder<TPermission, TDomainObject, SecurityPath<TDomainObject>.AndSecurityPath>(builderFactory, securityPath, securityContextRestrictions)
{
    protected override Expression<Func<TArg1, TArg2, bool>> BuildOperation<TArg1, TArg2>(
        Expression<Func<TArg1, TArg2, bool>> arg1,
        Expression<Func<TArg1, TArg2, bool>> arg2) => arg1.BuildAnd(arg2);
}
