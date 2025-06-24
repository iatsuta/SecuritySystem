using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Builders.AccessorsBuilder;

public class SingleContextFilterBuilder<TPermission, TDomainObject, TSecurityContext>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory<Guid> hierarchicalObjectExpanderFactory,
    SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction)
    : ByIdentsFilterBuilder<TPermission, TDomainObject, TSecurityContext>(permissionSystem, hierarchicalObjectExpanderFactory, securityPath, securityContextRestriction)
    where TSecurityContext : class, ISecurityContext
{
    private readonly IExpressionEvaluator expressionEvaluator =
        expressionEvaluatorStorage.GetForType(typeof(SingleContextFilterBuilder<TPermission, TDomainObject, TSecurityContext>));

    protected override IEnumerable<TSecurityContext> GetSecurityObjects(TDomainObject domainObject)
    {
        var securityObject = this.expressionEvaluator.Evaluate(securityPath.Expression, domainObject);

        if (securityObject != null)
        {
            yield return securityObject;
        }
    }
}