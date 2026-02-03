using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;

using HierarchicalExpand;

using SecuritySystem.Builders._Factory;
using SecuritySystem.Builders._Filter;
using SecuritySystem.ExternalSystem;

namespace SecuritySystem.Builders.AccessorsBuilder;

public class AccessorsFilterBuilderFactory<TDomainObject>(IServiceProxyFactory serviceProxyFactory, IEnumerable<IPermissionSystem> permissionSystems) :
    IAccessorsFilterFactory<TDomainObject>
{
    public AccessorsFilterInfo<TDomainObject> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule, SecurityPath<TDomainObject> securityPath)
    {
        var accessorsFilterInfoList = permissionSystems.Select(permissionSystem =>
        {
            var factoryType = typeof(AccessorsFilterBuilderFactory<,>).MakeGenericType(typeof(TDomainObject), permissionSystem.PermissionType);

            var factory = serviceProxyFactory.Create<IAccessorsFilterFactory<TDomainObject>>(factoryType, permissionSystem);

            return factory.CreateFilter(securityRule, securityPath);
        }).ToList();

        return new AccessorsFilterInfo<TDomainObject>(
            domainObject => accessorsFilterInfoList.SelectMany(accessorsFilterInfo => accessorsFilterInfo.GetAccessorsFunc(domainObject))
                                                   .Distinct(StringComparer.CurrentCultureIgnoreCase));
    }
}

public class AccessorsFilterBuilderFactory<TDomainObject, TPermission>(
    IIdentityInfoSource identityInfoSource,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory) :
    FilterBuilderFactoryBase<TDomainObject, AccessorsFilterBuilder<TDomainObject, TPermission>>(identityInfoSource),
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

                    var permissionSource = permissionSystem.GetPermissionSource(securityRule);

                    return permissionSource.GetAccessors(filter);
                }));

        return new AccessorsFilterInfo<TDomainObject>(v => getAccessorsFunc.Value(v));
    }

    protected override AccessorsFilterBuilder<TDomainObject, TPermission> CreateBuilder(
        SecurityPath<TDomainObject>.ConditionPath securityPath)
    {
        return new ConditionFilterBuilder<TDomainObject, TPermission>(expressionEvaluatorStorage, securityPath);
    }

    protected override AccessorsFilterBuilder<TDomainObject, TPermission> CreateBuilder<TSecurityContext, TSecurityContextIdent>(
        SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
        IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
    {
        return new SingleContextFilterBuilder<TDomainObject, TPermission, TSecurityContext, TSecurityContextIdent>(
            expressionEvaluatorStorage,
            permissionSystem,
            hierarchicalObjectExpanderFactory,
            securityPath,
            securityContextRestriction,
            identityInfo);
    }

    protected override AccessorsFilterBuilder<TDomainObject, TPermission> CreateBuilder<TSecurityContext, TSecurityContextIdent>(
        SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
        IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
    {
        return new ManyContextFilterBuilder<TDomainObject, TPermission, TSecurityContext, TSecurityContextIdent>(
            expressionEvaluatorStorage,
            permissionSystem,
            hierarchicalObjectExpanderFactory,
            securityPath,
            securityContextRestriction,
            identityInfo);
    }

    protected override AccessorsFilterBuilder<TDomainObject, TPermission> CreateBuilder(
        SecurityPath<TDomainObject>.OrSecurityPath securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        return new OrFilterBuilder<TDomainObject, TPermission>(this, securityPath, securityContextRestrictions);
    }

    protected override AccessorsFilterBuilder<TDomainObject, TPermission> CreateBuilder(
        SecurityPath<TDomainObject>.AndSecurityPath securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        return new AndFilterBuilder<TDomainObject, TPermission>(this, securityPath, securityContextRestrictions);
    }

    protected override AccessorsFilterBuilder<TDomainObject, TPermission> CreateBuilder<TNestedObject>(
        SecurityPath<TDomainObject>.NestedManySecurityPath<TNestedObject> securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        var nestedBuilderFactory = new AccessorsFilterBuilderFactory<TNestedObject, TPermission>(
            identityInfoSource,
            expressionEvaluatorStorage,
            permissionSystem,
            hierarchicalObjectExpanderFactory);

        return new NestedManyFilterBuilder<TDomainObject, TPermission, TNestedObject>(
            expressionEvaluatorStorage,
            nestedBuilderFactory,
            securityPath,
            securityContextRestrictions);
    }
}
