using CommonFramework.ExpressionEvaluate;

using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Builders.AccessorsBuilder;

public class SingleContextFilterBuilder<TPermission, TDomainObject, TSecurityContext, TIdent>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory,
    SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
    IdentityInfo<TSecurityContext, TIdent> identityInfo)
    : ByIdentsFilterBuilder<TPermission, TDomainObject, TSecurityContext, TIdent>(permissionSystem, hierarchicalObjectExpanderFactory, securityPath,
        securityContextRestriction, identityInfo)
    where TSecurityContext : class, ISecurityContext
    where TIdent : notnull
{
    private readonly IExpressionEvaluator expressionEvaluator =
        expressionEvaluatorStorage.GetForType(typeof(SingleContextFilterBuilder<TPermission, TDomainObject, TSecurityContext, TIdent>));

    protected override IEnumerable<TSecurityContext> GetSecurityObjects(TDomainObject domainObject)
    {
        var securityObject = this.expressionEvaluator.Evaluate(securityPath.Expression, domainObject);

        if (securityObject != null)
        {
            yield return securityObject;
        }
    }
}