using System.Linq.Expressions;

namespace SecuritySystem.TemplatePermission;

public class AvailablePermissionFilter(DateTime today)
{
    public string? PrincipalName { get; set; }

    public List<Guid>? SecurityRoleIdents { get; set; }

    public required IReadOnlyDictionary<Guid, (bool, Expression<Func<Guid, bool>>)> RestrictionFilters { get; set; }


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
