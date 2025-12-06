using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;
using HierarchicalExpand;
using SecuritySystem.ExternalSystem;

namespace SecuritySystem.Builders.AccessorsBuilder;

public class ManyContextFilterBuilder<TPermission, TDomainObject, TSecurityContext, TSecurityContextIdent>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory,
    SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
    IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
    : ByIdentsFilterBuilder<TPermission, TDomainObject, TSecurityContext, TSecurityContextIdent>(permissionSystem, hierarchicalObjectExpanderFactory, securityPath, securityContextRestriction, identityInfo)
    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
{
    private readonly IExpressionEvaluator expressionEvaluator =
        expressionEvaluatorStorage.GetForType(typeof(ManyContextFilterBuilder<TPermission, TDomainObject, TSecurityContext, TSecurityContextIdent>));

    protected override IEnumerable<TSecurityContext> GetSecurityObjects(TDomainObject domainObject) =>

        this.expressionEvaluator.Evaluate(securityPath.Expression, domainObject).EmptyIfNull();
}