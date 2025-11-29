using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;

using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionSource<TPrincipal, TPermission>(
    IServiceProvider serviceProvider,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IIdentityInfoSource identityInfoSource,
    IUserNameResolver<TPrincipal> userNameResolver,
    IQueryableSource queryableSource,
    TimeProvider timeProvider,
    UserSourceInfo<TPrincipal> userSourceInfo,
    SecurityRuleCredential defaultSecurityRuleCredential,
	VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo,
	DomainSecurityRule.RoleBaseSecurityRule securityRule) : IPermissionSource<TPermission>
    where TPermission : class
{
    private readonly IExpressionEvaluator expressionEvaluator = expressionEvaluatorStorage.GetForType(typeof(VirtualPermissionSource<TPrincipal, TPermission>));

    private readonly Expression<Func<TPermission, string>> fullNamePath = bindingInfo.PrincipalPath.Select(userSourceInfo.Name.Path);

    public bool HasAccess() => this.GetPermissionQuery().Any();

    public List<Dictionary<Type, Array>> GetPermissions(IEnumerable<Type> securityContextTypes)
    {
        var permissions = this.GetPermissionQuery(null).ToList();

        var restrictionFilterInfoList = securityRule.GetSafeSecurityContextRestrictionFilters().ToList();

        return permissions.Select(permission => this.ConvertPermission(permission, securityContextTypes, restrictionFilterInfoList)).ToList();
    }

    public IQueryable<TPermission> GetPermissionQuery() => this.GetPermissionQuery(null);

    private IQueryable<TPermission> GetPermissionQuery(SecurityRuleCredential? customSecurityRuleCredential)
    {
        var lazyToday = LazyHelper.Create(() => timeProvider.GetLocalNow().Date);

        //TODO: inject SecurityContextRestrictionFilterInfo
        return queryableSource
            .GetQueryable<TPermission>()
            .Where(bindingInfo.GetFilter(serviceProvider))
            .PipeMaybe(bindingInfo.StartDateFilter, (q, filter) => q.Where(filter.Select(startDate => startDate <= lazyToday.Value)))
            .PipeMaybe(bindingInfo.EndDateFilter, (q, filter) => q.Where(filter.Select(endDate => endDate == null || lazyToday.Value <= endDate)))
            .PipeMaybe(
                userNameResolver.Resolve(customSecurityRuleCredential ?? securityRule.CustomCredential ?? defaultSecurityRuleCredential),
                (q, principalName) => q.Where(this.fullNamePath.Select(name => name == principalName)));
    }

    public IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> permissionFilter) =>
        this.GetPermissionQuery(new SecurityRuleCredential.AnyUserCredential()).Where(permissionFilter).Select(this.fullNamePath);

    private Dictionary<Type, Array> ConvertPermission(
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

                var identityInfo = identityInfoSource.GetIdentityInfo(securityContextType);

                var getIdentsArrayExpr = bindingInfo.GetRestrictionsArrayExpr(identityInfo, pureFilter);

                return expressionEvaluator.Evaluate(getIdentsArrayExpr, permission);
            });
    }
}