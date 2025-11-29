using System.Linq.Expressions;

using CommonFramework;

using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Builders.AccessorsBuilder;

public abstract class ByIdentsFilterBuilder<TPermission, TDomainObject, TSecurityContext, TIdent>(
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory,
    IContextSecurityPath contextSecurityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
    IdentityInfo<TSecurityContext, TIdent> identityInfo) : AccessorsFilterBuilder<TPermission, TDomainObject>
    where TSecurityContext : class, ISecurityContext
    where TIdent : notnull
{
    public override Expression<Func<TPermission, bool>> GetAccessorsFilter(
        TDomainObject domainObject,
        HierarchicalExpandType expandType)
    {
        var securityObjects = this.GetSecurityObjects(domainObject).ToArray();

        var allowGrandAccess = securityContextRestriction?.Required != true;

        var grandAccessExpr = allowGrandAccess
                                  ? permissionSystem.GetGrandAccessExpr<TSecurityContext>()
                                  : _ => false;

        if (securityObjects.Any())
        {
            var securityIdents = hierarchicalObjectExpanderFactory
                                 .Create<TIdent>(typeof(TSecurityContext))
                                 .Expand(securityObjects.Select(identityInfo.Id.Getter), expandType.Reverse());

            return grandAccessExpr.BuildOr(permissionSystem.GetContainsIdentsExpr(securityIdents, securityContextRestriction?.Filter));
        }
        else
        {
            if (contextSecurityPath.Required)
            {
                return grandAccessExpr;
            }
            else
            {
                return _ => true;
            }
        }
    }

    protected abstract IEnumerable<TSecurityContext> GetSecurityObjects(TDomainObject domainObject);
}
