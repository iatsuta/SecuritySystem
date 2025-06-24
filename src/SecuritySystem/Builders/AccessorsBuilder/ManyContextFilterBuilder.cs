using CommonFramework;

using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Builders.AccessorsBuilder;

public class ManyContextFilterBuilder<TPermission, TDomainObject, TSecurityContext>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory<Guid> hierarchicalObjectExpanderFactory,
    SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction)
    : ByIdentsFilterBuilder<TPermission, TDomainObject, TSecurityContext>(permissionSystem, hierarchicalObjectExpanderFactory, securityPath, securityContextRestriction)
    where TSecurityContext : class, ISecurityContext
{
    private readonly IExpressionEvaluator expressionEvaluator =
        expressionEvaluatorStorage.GetForType(typeof(ManyContextFilterBuilder<TPermission, TDomainObject, TSecurityContext>));

    protected override IEnumerable<TSecurityContext> GetSecurityObjects(TDomainObject domainObject) =>

        this.expressionEvaluator.Evaluate(securityPath.Expression, domainObject).EmptyIfNull();
}