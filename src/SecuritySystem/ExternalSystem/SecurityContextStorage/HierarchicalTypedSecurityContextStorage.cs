using CommonFramework;

using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class HierarchicalTypedSecurityContextStorage<TSecurityContext, TIdent>(
    IQueryableSource queryableSource,
    LocalStorage<TSecurityContext, TIdent> localStorage,
    IdentityInfo<TSecurityContext, TIdent> identityInfo,
    ISecurityContextDisplayService<TSecurityContext> displayService,
    HierarchicalInfo<TSecurityContext> hierarchicalInfo)
    : TypedSecurityContextStorageBase<TSecurityContext, TIdent>(queryableSource, localStorage, identityInfo)
    where TSecurityContext : class, ISecurityContext
    where TIdent : notnull
{
    protected override SecurityContextData<TIdent> CreateSecurityContextData(TSecurityContext securityContext) =>

        new(identityInfo.IdFunc(securityContext), displayService.ToString(securityContext), hierarchicalInfo.ParentFunc(securityContext).Maybe(identityInfo.IdFunc));

    protected override IEnumerable<TSecurityContext> GetSecurityContextsWithMasterExpand(TSecurityContext startSecurityObject)
    {
        return startSecurityObject.GetAllElements(hierarchicalInfo.ParentFunc);
    }
}