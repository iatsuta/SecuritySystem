using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.Builders.QueryBuilder;

public class OrFilterBuilder<TPermission, TDomainObject>(
    SecurityFilterBuilderFactory<TPermission, TDomainObject> builderFactory,
    SecurityPath<TDomainObject>.OrSecurityPath securityPath,
    IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    : BinaryFilterBuilder<TPermission, TDomainObject, SecurityPath<TDomainObject>.OrSecurityPath>(builderFactory, securityPath, securityContextRestrictions)
{
    protected override Expression<Func<TArg1, TArg2, bool>> BuildOperation<TArg1, TArg2>(
        Expression<Func<TArg1, TArg2, bool>> arg1,
        Expression<Func<TArg1, TArg2, bool>> arg2) => arg1.BuildOr(arg2);
}
