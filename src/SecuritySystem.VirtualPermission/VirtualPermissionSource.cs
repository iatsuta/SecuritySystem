using System.Linq.Expressions;

using CommonFramework;

using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionSource<TPrincipal, TPermission>(
    IServiceProvider serviceProvider,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IUserNameResolver userNameResolver,
    IQueryableSource queryableSource,
    TimeProvider timeProvider,
    VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo,
    DomainSecurityRule.RoleBaseSecurityRule securityRule,
    SecurityRuleCredential defaultSecurityRuleCredential) : IPermissionSource<TPermission>
    where TPermission : class
{
    private readonly IExpressionEvaluator expressionEvaluator = expressionEvaluatorStorage.GetForType(typeof(VirtualPermissionSource<TPrincipal, TPermission>));

    private readonly Expression<Func<TPermission, string>> fullNamePath = bindingInfo.PrincipalPath.Select(bindingInfo.PrincipalNamePath);

    public bool HasAccess() => this.GetPermissionQuery().Any();

    public List<Dictionary<Type, List<Guid>>> GetPermissions(IEnumerable<Type> securityContextTypes)
    {
        var permissions = this.GetPermissionQuery(null).ToList();

        var restrictionFilterInfoList = securityRule.GetSafeSecurityContextRestrictionFilters().ToList();

        return permissions.Select(permission => this.ConvertPermission(permission, securityContextTypes, restrictionFilterInfoList)).ToList();
    }

    public IQueryable<TPermission> GetPermissionQuery() => this.GetPermissionQuery(null);

    private IQueryable<TPermission> GetPermissionQuery(SecurityRuleCredential? customSecurityRuleCredential)
    {
        //TODO: inject SecurityContextRestrictionFilterInfo
        return queryableSource
            .GetQueryable<TPermission>()
            .Where(bindingInfo.GetFilter(serviceProvider))
            .PipeMaybe(
                bindingInfo.PeriodFilter,
                (q, filter) =>
                {
                    var today = timeProvider.GetLocalNow().Date;

                    return q.Where(filter.Select(period => period.StartDate <= today && (period.EndDate == null || today <= period.EndDate)));
                })
            .PipeMaybe(
                userNameResolver.Resolve(customSecurityRuleCredential ?? securityRule.CustomCredential ?? defaultSecurityRuleCredential),
                (q, principalName) => q.Where(this.fullNamePath.Select(name => name == principalName)));
    }

    public IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> permissionFilter) =>
        this.GetPermissionQuery(new SecurityRuleCredential.AnyUserCredential()).Where(permissionFilter).Select(this.fullNamePath);

    private Dictionary<Type, List<Guid>> ConvertPermission(
        TPermission permission,
        IEnumerable<Type> securityContextTypes,
        IReadOnlyCollection<SecurityContextRestrictionFilterInfo> filterInfoList)
    {
        return securityContextTypes.ToDictionary(
            securityContextType => securityContextType,
            securityContextType =>
            {
                var filter = filterInfoList.SingleOrDefault(f => f.SecurityContextType == securityContextType);

                var pureFilter = filter?.GetBasePureFilter(serviceProvider);

                return this.expressionEvaluator.Evaluate(bindingInfo.GetRestrictionsExpr(securityContextType, pureFilter), permission).ToList();
            });
    }
}