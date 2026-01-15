using CommonFramework;
using CommonFramework.DependencyInjection;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;

using HierarchicalExpand;

using SecuritySystem.Builders._Factory;
using SecuritySystem.Builders._Filter;
using SecuritySystem.ExternalSystem;

namespace SecuritySystem.Builders.QueryBuilder;

public class SecurityFilterBuilderFactory<TDomainObject>(
    IServiceProxyFactory serviceProxyFactory,
    IEnumerable<IPermissionSystem> permissionSystems) :
    ISecurityFilterFactory<TDomainObject>
{
    public SecurityFilterInfo<TDomainObject> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule, SecurityPath<TDomainObject> securityPath)
    {
        var securityFilterInfoList = permissionSystems.Select(
            permissionSystem =>
            {
                var factoryType = typeof(SecurityFilterBuilderFactory<,>).MakeGenericType(typeof(TDomainObject), permissionSystem.PermissionType);

                var factory = serviceProxyFactory.Create<ISecurityFilterFactory<TDomainObject>>(
                    factoryType,
                    permissionSystem);

                return factory.CreateFilter(securityRule, securityPath);
            }).ToList();

        return new SecurityFilterInfo<TDomainObject>(
            q => securityFilterInfoList
                 .Match(
                     () => q.Where(_ => false),
                     filter => filter.InjectFunc(q),
                     filters => filters.Aggregate(q, (state, filter) => state.Union(filter.InjectFunc(q)))),

            domainObject => securityFilterInfoList.Any(filter => filter.HasAccessFunc(domainObject)));
    }
}

public class SecurityFilterBuilderFactory<TDomainObject, TPermission>(
    IIdentityInfoSource identityInfoSource,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory) :
    FilterBuilderFactoryBase<TDomainObject, SecurityFilterBuilder<TDomainObject, TPermission>>(identityInfoSource),
    ISecurityFilterFactory<TDomainObject>
{
    private readonly IExpressionEvaluator expressionEvaluator = expressionEvaluatorStorage.GetForType(typeof(SecurityFilterBuilderFactory<TDomainObject, TPermission>));

    public SecurityFilterInfo<TDomainObject> CreateFilter(
        DomainSecurityRule.RoleBaseSecurityRule securityRule,
        SecurityPath<TDomainObject> securityPath)
    {
        var securityContextRestrictions = securityRule.GetSafeSecurityContextRestrictions().ToList();

        var builder = this.CreateBuilder(securityPath, securityContextRestrictions);

        var permissionFilterExpression = builder.GetSecurityFilterExpression(securityRule.GetSafeExpandType());

        var permissionQuery = permissionSystem.GetPermissionSource(securityRule).GetPermissionQuery();

        var filterExpression =

            ExpressionEvaluateHelper.InlineEvaluate(ee =>

                ExpressionHelper.Create((TDomainObject domainObject) =>

                    permissionQuery.Any(permission => ee.Evaluate(permissionFilterExpression, domainObject, permission))));

        var lazyHasAccessFunc = LazyHelper.Create(() => filterExpression.UpdateBody(CacheContainsCallVisitor.Value).Pipe(this.expressionEvaluator.Compile));

        return new SecurityFilterInfo<TDomainObject>(
            q => q.Where(filterExpression),
            v => lazyHasAccessFunc.Value(v));
    }

    protected override SecurityFilterBuilder<TDomainObject, TPermission> CreateBuilder(
        SecurityPath<TDomainObject>.ConditionPath securityPath)
    {
        return new ConditionFilterBuilder<TDomainObject, TPermission>(securityPath);
    }

    protected override SecurityFilterBuilder<TDomainObject, TPermission> CreateBuilder<TSecurityContext, TSecurityContextIdent>(
        SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
        IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
    {
        return new SingleContextFilterBuilder<TDomainObject, TPermission, TSecurityContext, TSecurityContextIdent>(
            permissionSystem, hierarchicalObjectExpanderFactory, securityPath, securityContextRestriction, identityInfo);
    }

    protected override SecurityFilterBuilder<TDomainObject, TPermission> CreateBuilder<TSecurityContext, TSecurityContextIdent>(
        SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
        IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
    {
        return new ManyContextFilterBuilder<TDomainObject, TPermission, TSecurityContext, TSecurityContextIdent>(
            permissionSystem, hierarchicalObjectExpanderFactory, securityPath, securityContextRestriction, identityInfo);
    }

    protected override SecurityFilterBuilder<TDomainObject, TPermission> CreateBuilder(
        SecurityPath<TDomainObject>.OrSecurityPath securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        return new OrFilterBuilder<TDomainObject, TPermission>(this, securityPath, securityContextRestrictions);
    }

    protected override SecurityFilterBuilder<TDomainObject, TPermission> CreateBuilder(
        SecurityPath<TDomainObject>.AndSecurityPath securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        return new AndFilterBuilder<TDomainObject, TPermission>(this, securityPath, securityContextRestrictions);
    }

    protected override SecurityFilterBuilder<TDomainObject, TPermission> CreateBuilder<TNestedObject>(
        SecurityPath<TDomainObject>.NestedManySecurityPath<TNestedObject> securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        var nestedBuilderFactory = new SecurityFilterBuilderFactory<TNestedObject, TPermission>(
            identityInfoSource,
            expressionEvaluatorStorage,
            permissionSystem,
            hierarchicalObjectExpanderFactory);

        return new NestedManyFilterBuilder<TDomainObject, TPermission, TNestedObject>(
            nestedBuilderFactory,
            securityPath,
            securityContextRestrictions);
    }
}