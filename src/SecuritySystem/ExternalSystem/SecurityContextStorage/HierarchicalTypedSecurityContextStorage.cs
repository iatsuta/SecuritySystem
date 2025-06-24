using CommonFramework;

using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class HierarchicalTypedSecurityContextStorage<TSecurityContext>(
    IQueryableSource queryableSource,
    LocalStorage<TSecurityContext> localStorage,
    ISecurityContextDisplayService<TSecurityContext> displayService,
    HierarchicalInfo<TSecurityContext> hierarchicalInfo)
    : TypedSecurityContextStorageBase<TSecurityContext>(queryableSource, localStorage)
    where TSecurityContext : class, ISecurityContext
{
    protected override SecurityContextData CreateSecurityContextData(TSecurityContext securityContext) =>

        new(securityContext.Id, displayService.ToString(securityContext), hierarchicalInfo.ParentFunc(securityContext).Maybe(v => v.Id));

    protected override IEnumerable<TSecurityContext> GetSecurityContextsWithMasterExpand(TSecurityContext startSecurityObject)
    {
        return startSecurityObject.GetAllElements(hierarchicalInfo.ParentFunc);
    }
}