using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public record AvailablePermissionFilter<TSecurityContextObjectIdent>
{
    public required DateTime Date { get; init; }

    public string? PrincipalName { get; init; }

    public required IReadOnlyList<TSecurityContextObjectIdent>? SecurityRoleIdents { get; init; }

    public required IReadOnlyDictionary<TSecurityContextObjectIdent, (bool, Expression<Func<TSecurityContextObjectIdent, bool>>)> RestrictionFilters { get; init; }


    //public Expression<Func<TPermission, bool>> ToFilterExpression()
    //{
    //    return this.GetFilterExpressionElements().BuildAnd();
    //}

    //public IEnumerable<Expression<Func<TPermission, bool>>> GetFilterExpressionElements()
    //{
    //    yield return permission => permission.Period.Contains(today);

    //    if (this.PrincipalName != null)
    //    {
    //        yield return permission => this.PrincipalName == permission.TPrincipal.Name;
    //    }

    //    if (this.SecurityRoleIdents != null)
    //    {
    //        yield return permission => this.SecurityRoleIdents.Contains(permission.Role.Id);
    //    }

    //    foreach (var (securityContextTypeId, (allowGrandAccess, restrictionFilterExpr)) in this.RestrictionFilters)
    //    {
    //        var baseFilter =
    //            ExpressionEvaluateHelper.InlineEvaluate(ee =>
    //                                                        ExpressionHelper
    //                                                            .Create((TPermission permission) =>
    //                                                                        permission.Restrictions.Any(r => r.SecurityContextType.Id
    //                                                                                    == securityContextTypeId
    //                                                                                    && ee.Evaluate(
    //                                                                                        restrictionFilterExpr,
    //                                                                                        r.SecurityContextId))));

    //        if (allowGrandAccess)
    //        {
    //            var grandAccessExpr = ExpressionHelper.Create(
    //                (TPermission permission) =>
    //                    permission.Restrictions.All(r => r.SecurityContextType.Id != securityContextTypeId));

    //            yield return baseFilter.BuildOr(grandAccessExpr);
    //        }
    //        else
    //        {
    //            yield return baseFilter;
    //        }
    //    }
    //}
}
