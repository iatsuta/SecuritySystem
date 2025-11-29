using CommonFramework;

using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class HierarchicalTypedSecurityContextStorage<TSecurityContext, TIdent>(
	IQueryableSource queryableSource,
	IIdentityInfoSource identityInfoSource,
	LocalStorage<TSecurityContext, TIdent> localStorage,
	ISecurityContextDisplayService<TSecurityContext> displayService,
	HierarchicalInfo<TSecurityContext> hierarchicalInfo)
	: TypedSecurityContextStorageBase<TSecurityContext, TIdent>(queryableSource, identityInfoSource, localStorage)
	where TSecurityContext : class, ISecurityContext
	where TIdent : notnull
{
	protected override SecurityContextData<TIdent> CreateSecurityContextData(TSecurityContext securityContext) =>

		new(this.IdentityInfo.Id.Getter(securityContext), displayService.ToString(securityContext),
			hierarchicalInfo.ParentFunc(securityContext).Maybe(IdentityInfo.Id.Getter));

	protected override IEnumerable<TSecurityContext> GetSecurityContextsWithMasterExpand(TSecurityContext startSecurityObject)
	{
		return startSecurityObject.GetAllElements(hierarchicalInfo.ParentFunc);
	}
}