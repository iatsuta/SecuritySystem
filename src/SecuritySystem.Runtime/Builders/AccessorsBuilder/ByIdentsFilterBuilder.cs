using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.IdentitySource;

using HierarchicalExpand;

using SecuritySystem.ExternalSystem;

namespace SecuritySystem.Builders.AccessorsBuilder;

public abstract class ByIdentsFilterBuilder<TDomainObject, TPermission, TSecurityContext, TSecurityContextIdent>(
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory,
    IContextSecurityPath contextSecurityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
    IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo) : AccessorsFilterBuilder<TDomainObject, TPermission>
    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
{
    private readonly IPermissionRestrictionSource<TPermission, TSecurityContextIdent> permissionRestrictionSource =
        permissionSystem.GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(securityContextRestriction?.Filter);

    public override Expression<Func<TPermission, bool>> GetAccessorsFilter(TDomainObject domainObject, HierarchicalExpandType expandType)
    {
        var securityObjects = this.GetSecurityObjects(domainObject).ToArray();

        var allowsUnrestrictedAccess = securityContextRestriction?.Required != true;

        var unrestrictedFilter = allowsUnrestrictedAccess
            ? permissionRestrictionSource.GetUnrestrictedFilter()
            : _ => false;

        if (securityObjects.Any())
        {
            var securityIdents = hierarchicalObjectExpanderFactory
                .Create<TSecurityContextIdent>(typeof(TSecurityContext))
                .Expand(securityObjects.Select(identityInfo.Id.Getter), expandType.Reverse());

            return unrestrictedFilter.BuildOr(permissionRestrictionSource.GetContainsIdentsExpr(securityIdents));
        }
        else
        {
            if (contextSecurityPath.Required)
            {
                return unrestrictedFilter;
            }
            else
            {
                return _ => true;
            }
        }
    }

    protected abstract IEnumerable<TSecurityContext> GetSecurityObjects(TDomainObject domainObject);
}