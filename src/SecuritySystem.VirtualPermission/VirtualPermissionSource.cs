using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;
using CommonFramework.DependencyInjection;

using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionSource<TPermission>(
    IServiceProxyFactory serviceProxyFactory,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IPermissionBindingInfoSource bindingInfoSource,
    VirtualPermissionBindingInfo<TPermission> virtualBindingInfo,
    DomainSecurityRule.RoleBaseSecurityRule securityRule) : IPermissionSource<TPermission>
    where TPermission : class
{
    private readonly Lazy<IPermissionSource<TPermission>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo(bindingInfo.PrincipalType);

        var innerServiceType = typeof(VirtualPermissionSource<,>).MakeGenericType(
            bindingInfo.PrincipalType,
            bindingInfo.PermissionType);

        return serviceProxyFactory.Create<IPermissionSource<TPermission>>(
            innerServiceType,
            bindingInfo,
            virtualBindingInfo,
            principalVisualIdentityInfo,
            securityRule);
    });

    private IPermissionSource<TPermission> InnerService => this.lazyInnerService.Value;

    public bool HasAccess() => this.InnerService.HasAccess();

    public List<Dictionary<Type, Array>> GetPermissions(IEnumerable<Type> securityContextTypes) =>
        this.InnerService.GetPermissions(securityContextTypes);

    public IQueryable<TPermission> GetPermissionQuery() => this.InnerService.GetPermissionQuery();

    public IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> permissionFilter) => this.InnerService.GetAccessors(permissionFilter);
}

public class VirtualPermissionSource<TPrincipal, TPermission>(
    IServiceProvider serviceProvider,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IIdentityInfoSource identityInfoSource,
    IUserNameResolver<TPrincipal> userNameResolver,
    IQueryableSource queryableSource,
    TimeProvider timeProvider,
    SecurityRuleCredential defaultSecurityRuleCredential,
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    VirtualPermissionBindingInfo<TPermission> virtualBindingInfo,
    VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo,
    DomainSecurityRule.RoleBaseSecurityRule securityRule) : IPermissionSource<TPermission>
    where TPermission : class
{
    private readonly IExpressionEvaluator expressionEvaluator = expressionEvaluatorStorage.GetForType(typeof(VirtualPermissionSource<TPrincipal, TPermission>));

    private readonly Expression<Func<TPermission, string>> fullNamePath = bindingInfo.Principal.Path.Select(principalVisualIdentityInfo.Name.Path);

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
        //TODO: inject SecurityContextRestrictionFilterInfo
        return queryableSource
            .GetQueryable<TPermission>()
            .Where(virtualBindingInfo.GetFilter(serviceProvider))
            .Where(bindingInfo.GetPeriodFilter(timeProvider.GetLocalNow().Date))
            .PipeMaybe(
                userNameResolver.Resolve(customSecurityRuleCredential ?? securityRule.CustomCredential ?? defaultSecurityRuleCredential),
                (q, principalName) => q.Where(this.fullNamePath.Select(name => name == principalName)));
    }

    public IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> permissionFilter) =>
        this.GetPermissionQuery(new SecurityRuleCredential.AnyUserCredential()).Where(permissionFilter).Select(this.fullNamePath).Distinct();

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

                var getIdentsArrayExpr = virtualBindingInfo.GetRestrictionsArrayExpr(identityInfo, pureFilter);

                return expressionEvaluator.Evaluate(getIdentsArrayExpr, permission);
            });
    }
}