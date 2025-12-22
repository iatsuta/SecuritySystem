using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;

using SecuritySystem.ExternalSystem;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionRestrictionSource<TPermission, TSecurityContext, TSecurityContextIdent>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    VirtualPermissionBindingInfo<TPermission> virtualBindingInfo,
    SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo) : IPermissionRestrictionSource<TPermission, TSecurityContextIdent>

    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
    where TPermission : notnull
{
    private readonly IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo =
        identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextIdent>();

    public Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetIdentsExpr() =>
        virtualBindingInfo.GetRestrictionsExpr(identityInfo, restrictionFilterInfo?.GetPureFilter(serviceProvider));

    public Expression<Func<TPermission, bool>> GetGrandAccessExpr() => this.GetManyGrandAccessExpr().BuildAnd();

    public Expression<Func<TPermission, bool>> GetContainsIdentsExpr(IEnumerable<TSecurityContextIdent> idents) =>
        this.GetManyContainsIdentsExpr(idents).BuildOr();

    private IEnumerable<Expression<Func<TPermission, bool>>> GetManyGrandAccessExpr()
    {
        foreach (var restrictionPath in virtualBindingInfo.Restrictions)
        {
            if (restrictionPath is Expression<Func<TPermission, TSecurityContext?>> singlePath)
            {
                yield return singlePath.Select(securityContext => securityContext == null);
            }
            else if (restrictionPath is Expression<Func<TPermission, IEnumerable<TSecurityContext>>> manyPath)
            {
                yield return manyPath.Select(securityContexts => !securityContexts.Any());
            }
        }
    }

    private IEnumerable<Expression<Func<TPermission, bool>>> GetManyContainsIdentsExpr(IEnumerable<TSecurityContextIdent> idents)
    {
        var filterExpr = identityInfo.CreateContainsFilter(idents.ToArray());

        foreach (var restrictionPath in virtualBindingInfo.Restrictions)
        {
            if (restrictionPath is Expression<Func<TPermission, TSecurityContext>> singlePath)
            {
                if (restrictionFilterInfo == null)
                {
                    yield return singlePath.Select(filterExpr);
                }
                else
                {
                    var securityContextFilter = restrictionFilterInfo.GetPureFilter(serviceProvider)
                        .BuildAnd(filterExpr);

                    yield return singlePath.Select(securityContextFilter);
                }
            }
            else if (restrictionPath is Expression<Func<TPermission, IEnumerable<TSecurityContext>>> manyPath)
            {
                yield return ExpressionEvaluateHelper.InlineEvaluate(ee =>
                {
                    if (restrictionFilterInfo == null)
                    {
                        return manyPath.Select(securityContexts => securityContexts.Any(securityContext => ee.Evaluate(filterExpr, securityContext)));
                    }
                    else
                    {
                        var securityContextFilter = restrictionFilterInfo.GetPureFilter(serviceProvider).ToEnumerableAny()
                            .BuildAnd(securityContexts => securityContexts.Any(securityContext => ee.Evaluate(filterExpr, securityContext)));

                        return manyPath.Select(securityContextFilter);
                    }
                });
            }
        }
    }
}