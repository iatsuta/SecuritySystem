using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Builders.AccessorsBuilder;

public class ManyContextFilterBuilder<TPermission, TDomainObject, TSecurityContext, TIdent>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory,
    SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
    IdentityInfo<TSecurityContext, TIdent> identityInfo)
    : ByIdentsFilterBuilder<TPermission, TDomainObject, TSecurityContext, TIdent>(permissionSystem, hierarchicalObjectExpanderFactory, securityPath, securityContextRestriction, identityInfo)
    where TSecurityContext : class, ISecurityContext
    where TIdent : notnull
{
    private readonly IExpressionEvaluator expressionEvaluator =
        expressionEvaluatorStorage.GetForType(typeof(ManyContextFilterBuilder<TPermission, TDomainObject, TSecurityContext, TIdent>));

    protected override IEnumerable<TSecurityContext> GetSecurityObjects(TDomainObject domainObject) =>

        this.expressionEvaluator.Evaluate(securityPath.Expression, domainObject).EmptyIfNull();
}