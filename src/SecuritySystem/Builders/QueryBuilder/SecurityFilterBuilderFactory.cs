﻿using CommonFramework;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Builders._Factory;
using SecuritySystem.Builders._Filter;
using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Builders.QueryBuilder;

public class SecurityFilterBuilderFactory<TDomainObject>(
    IServiceProvider serviceProvider,
    IEnumerable<IPermissionSystem> permissionSystems) :
    ISecurityFilterFactory<TDomainObject>
{
    public SecurityFilterInfo<TDomainObject> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule, SecurityPath<TDomainObject> securityPath)
    {
        var securityFilterInfoList = permissionSystems.Select(
            permissionSystem =>
            {
                var factoryType = typeof(SecurityFilterBuilderFactory<,>).MakeGenericType(
                    permissionSystem.PermissionType,
                    typeof(TDomainObject));

                var factory = (ISecurityFilterFactory<TDomainObject>)ActivatorUtilities.CreateInstance(
                    serviceProvider,
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

public class SecurityFilterBuilderFactory<TPermission, TDomainObject>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory<Guid> hierarchicalObjectExpanderFactory) :
    FilterBuilderFactoryBase<TDomainObject, SecurityFilterBuilder<TPermission, TDomainObject>>,
    ISecurityFilterFactory<TDomainObject>
{
    private readonly IExpressionEvaluator expressionEvaluator = expressionEvaluatorStorage.GetForType(typeof(SecurityFilterBuilderFactory<TPermission, TDomainObject>));

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

    protected override SecurityFilterBuilder<TPermission, TDomainObject> CreateBuilder(
        SecurityPath<TDomainObject>.ConditionPath securityPath)
    {
        return new ConditionFilterBuilder<TPermission, TDomainObject>(securityPath);
    }

    protected override SecurityFilterBuilder<TPermission, TDomainObject> CreateBuilder<TSecurityContext>(
        SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction)
    {
        return new SingleContextFilterBuilder<TPermission, TDomainObject, TSecurityContext>(
            permissionSystem,
            hierarchicalObjectExpanderFactory,
            securityPath,
            securityContextRestriction);
    }

    protected override SecurityFilterBuilder<TPermission, TDomainObject> CreateBuilder<TSecurityContext>(
        SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction)
    {
        return new ManyContextFilterBuilder<TPermission, TDomainObject, TSecurityContext>(
            permissionSystem, hierarchicalObjectExpanderFactory, securityPath, securityContextRestriction);
    }

    protected override SecurityFilterBuilder<TPermission, TDomainObject> CreateBuilder(
        SecurityPath<TDomainObject>.OrSecurityPath securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        return new OrFilterBuilder<TPermission, TDomainObject>(this, securityPath, securityContextRestrictions);
    }

    protected override SecurityFilterBuilder<TPermission, TDomainObject> CreateBuilder(
        SecurityPath<TDomainObject>.AndSecurityPath securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        return new AndFilterBuilder<TPermission, TDomainObject>(this, securityPath, securityContextRestrictions);
    }

    protected override SecurityFilterBuilder<TPermission, TDomainObject> CreateBuilder<TNestedObject>(
        SecurityPath<TDomainObject>.NestedManySecurityPath<TNestedObject> securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        var nestedBuilderFactory = new SecurityFilterBuilderFactory<TPermission, TNestedObject>(
            expressionEvaluatorStorage,
            permissionSystem,
            hierarchicalObjectExpanderFactory);

        return new NestedManyFilterBuilder<TPermission, TDomainObject, TNestedObject>(
            nestedBuilderFactory,
            securityPath,
            securityContextRestrictions);
    }
}