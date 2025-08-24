using CommonFramework;

using SecuritySystem.Builders._Factory;
using SecuritySystem.Builders._Filter;
using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;
using SecuritySystem.PermissionOptimization;
using SecuritySystem.Services;

namespace SecuritySystem.Builders.MaterializedBuilder;

public class SecurityFilterBuilderFactory<TDomainObject>(
    IIdentityInfoSource identityInfoSource,
	IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IEnumerable<IPermissionSystem> permissionSystems,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory,
    IRuntimePermissionOptimizationService permissionOptimizationService) :
    FilterBuilderFactoryBase<TDomainObject, SecurityFilterBuilder<TDomainObject>>(identityInfoSource),
    ISecurityFilterFactory<TDomainObject>
{
    private readonly IExpressionEvaluator expressionEvaluator =
        expressionEvaluatorStorage.GetForType(typeof(SecurityFilterBuilderFactory<TDomainObject>));

    public SecurityFilterInfo<TDomainObject> CreateFilter(
        DomainSecurityRule.RoleBaseSecurityRule securityRule,
        SecurityPath<TDomainObject> securityPath)
    {
        var securityTypes = securityPath.GetUsedTypes();

        var securityContextRestrictions = securityRule.GetSafeSecurityContextRestrictions().ToList();

        var rawPermissions = permissionSystems
                             .SelectMany(ps => ps.GetPermissionSource(securityRule).GetPermissions(securityTypes))
                             .ToList();

        var optimizedPermissions = permissionOptimizationService.Optimize(rawPermissions);

        var expandedPermissions =
            optimizedPermissions.Select(permission => this.TryExpandPermission(permission, securityRule.GetSafeExpandType()));

        var builder = this.CreateBuilder(securityPath, securityContextRestrictions);

        var filterExpression = expandedPermissions.BuildOr(builder.GetSecurityFilterExpression).ExpandConst();

        var lazyHasAccessFunc = LazyHelper.Create(
            () => filterExpression.UpdateBody(CacheContainsCallVisitor.Value).Pipe(this.expressionEvaluator.Compile));

        return new SecurityFilterInfo<TDomainObject>(
            q => q.Where(filterExpression),
            v => lazyHasAccessFunc.Value(v));
    }

    protected override SecurityFilterBuilder<TDomainObject> CreateBuilder(SecurityPath<TDomainObject>.ConditionPath securityPath)
    {
        return new ConditionFilterBuilder<TDomainObject>(securityPath);
    }

    protected override SecurityFilterBuilder<TDomainObject> CreateBuilder<TSecurityContext, TIdent>(
        SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
        IdentityInfo<TSecurityContext, TIdent> identityInfo)
    {
        return new SingleContextFilterBuilder<TDomainObject, TSecurityContext, TIdent>(securityPath, securityContextRestriction, identityInfo);
    }

    protected override SecurityFilterBuilder<TDomainObject> CreateBuilder<TSecurityContext, TIdent>(
        SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
        IdentityInfo<TSecurityContext, TIdent> identityInfo)
    {
        return new ManyContextFilterBuilder<TDomainObject, TSecurityContext, TIdent>(securityPath, securityContextRestriction, identityInfo);
    }

    protected override SecurityFilterBuilder<TDomainObject> CreateBuilder(SecurityPath<TDomainObject>.OrSecurityPath securityPath, IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        return new OrFilterBuilder<TDomainObject>(this, securityPath, securityContextRestrictions);
    }

    protected override SecurityFilterBuilder<TDomainObject> CreateBuilder(SecurityPath<TDomainObject>.AndSecurityPath securityPath, IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        return new AndFilterBuilder<TDomainObject>(this, securityPath, securityContextRestrictions);
    }

    protected override SecurityFilterBuilder<TDomainObject> CreateBuilder<TNestedObject>(
        SecurityPath<TDomainObject>.NestedManySecurityPath<TNestedObject> securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        var nestedBuilderFactory = new SecurityFilterBuilderFactory<TNestedObject>(
            identityInfoSource,
            expressionEvaluatorStorage,
            permissionSystems,
            hierarchicalObjectExpanderFactory,
            permissionOptimizationService);

        return new NestedManyFilterBuilder<TDomainObject, TNestedObject>(nestedBuilderFactory, securityPath, securityContextRestrictions);
    }

    private Dictionary<Type, Array> TryExpandPermission(
        Dictionary<Type, Array> permission,
        HierarchicalExpandType expandType)
    {
        return permission.ToDictionary(
            pair => pair.Key,
            pair => hierarchicalObjectExpanderFactory.Create(pair.Key).Expand(pair.Value, expandType));
    }
}
