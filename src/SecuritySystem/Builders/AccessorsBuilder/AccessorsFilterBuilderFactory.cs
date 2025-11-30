using CommonFramework;
using CommonFramework.ExpressionEvaluate;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Builders._Factory;
using SecuritySystem.Builders._Filter;
using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.Builders.AccessorsBuilder;

public class AccessorsFilterBuilderFactory<TDomainObject>(IServiceProvider serviceProvider, IEnumerable<IPermissionSystem> permissionSystems) :
    IAccessorsFilterFactory<TDomainObject>
{
    public AccessorsFilterInfo<TDomainObject> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule, SecurityPath<TDomainObject> securityPath)
    {
        var accessorsFilterInfoList = permissionSystems.Select(permissionSystem =>
        {
            var factoryType = typeof(AccessorsFilterBuilderFactory<,>).MakeGenericType(permissionSystem.PermissionType, typeof(TDomainObject));

            var factory = (IAccessorsFilterFactory<TDomainObject>)ActivatorUtilities.CreateInstance(serviceProvider, factoryType, permissionSystem);

            return factory.CreateFilter(securityRule, securityPath);
        }).ToList();

        return new AccessorsFilterInfo<TDomainObject>(
            domainObject => accessorsFilterInfoList.SelectMany(accessorsFilterInfo => accessorsFilterInfo.GetAccessorsFunc(domainObject))
                                                   .Distinct(StringComparer.CurrentCultureIgnoreCase));
    }
}

public class AccessorsFilterBuilderFactory<TPermission, TDomainObject>(
    IIdentityInfoSource identityInfoSource,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory) :
    FilterBuilderFactoryBase<TDomainObject, AccessorsFilterBuilder<TPermission, TDomainObject>>(identityInfoSource),
    IAccessorsFilterFactory<TDomainObject>
{
    public AccessorsFilterInfo<TDomainObject> CreateFilter(
        DomainSecurityRule.RoleBaseSecurityRule securityRule,
        SecurityPath<TDomainObject> securityPath)
    {
        var securityContextRestrictions = securityRule.GetSafeSecurityContextRestrictions().ToList();

        var builder = this.CreateBuilder(securityPath, securityContextRestrictions);

        var getAccessorsFunc = LazyHelper.Create(
            () => FuncHelper.Create(
                (TDomainObject domainObject) =>
                {
                    var filter = builder.GetAccessorsFilter(domainObject, securityRule.GetSafeExpandType());

                    return permissionSystem.GetPermissionSource(securityRule).GetAccessors(filter);
                }));

        return new AccessorsFilterInfo<TDomainObject>(v => getAccessorsFunc.Value(v));
    }

    protected override AccessorsFilterBuilder<TPermission, TDomainObject> CreateBuilder(
        SecurityPath<TDomainObject>.ConditionPath securityPath)
    {
        return new ConditionFilterBuilder<TPermission, TDomainObject>(expressionEvaluatorStorage, securityPath);
    }

    protected override AccessorsFilterBuilder<TPermission, TDomainObject> CreateBuilder<TSecurityContext, TSecurityContextIdent>(
        SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
        IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
    {
        return new SingleContextFilterBuilder<TPermission, TDomainObject, TSecurityContext, TSecurityContextIdent>(
            expressionEvaluatorStorage,
            permissionSystem,
            hierarchicalObjectExpanderFactory,
            securityPath,
            securityContextRestriction,
            identityInfo);
    }

    protected override AccessorsFilterBuilder<TPermission, TDomainObject> CreateBuilder<TSecurityContext, TSecurityContextIdent>(
        SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
        IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
    {
        return new ManyContextFilterBuilder<TPermission, TDomainObject, TSecurityContext, TSecurityContextIdent>(
            expressionEvaluatorStorage,
            permissionSystem,
            hierarchicalObjectExpanderFactory,
            securityPath,
            securityContextRestriction,
            identityInfo);
    }

    protected override AccessorsFilterBuilder<TPermission, TDomainObject> CreateBuilder(
        SecurityPath<TDomainObject>.OrSecurityPath securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        return new OrFilterBuilder<TPermission, TDomainObject>(this, securityPath, securityContextRestrictions);
    }

    protected override AccessorsFilterBuilder<TPermission, TDomainObject> CreateBuilder(
        SecurityPath<TDomainObject>.AndSecurityPath securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        return new AndFilterBuilder<TPermission, TDomainObject>(this, securityPath, securityContextRestrictions);
    }

    protected override AccessorsFilterBuilder<TPermission, TDomainObject> CreateBuilder<TNestedObject>(
        SecurityPath<TDomainObject>.NestedManySecurityPath<TNestedObject> securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        var nestedBuilderFactory = new AccessorsFilterBuilderFactory<TPermission, TNestedObject>(
            identityInfoSource,
            expressionEvaluatorStorage,
            permissionSystem,
            hierarchicalObjectExpanderFactory);

        return new NestedManyFilterBuilder<TPermission, TDomainObject, TNestedObject>(
            expressionEvaluatorStorage,
            nestedBuilderFactory,
            securityPath,
            securityContextRestrictions);
    }
}
