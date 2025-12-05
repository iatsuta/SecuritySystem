using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using HierarchicalExpand;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class HierarchicalTypedSecurityContextStorage<TSecurityContext, TSecurityContextIdent>(
	IQueryableSource queryableSource,
	IIdentityInfoSource identityInfoSource,
	LocalStorage<TSecurityContext, TSecurityContextIdent> localStorage,
	IDomainObjectDisplayService displayService,
	HierarchicalInfo<TSecurityContext> hierarchicalInfo)
	: TypedSecurityContextStorageBase<TSecurityContext, TSecurityContextIdent>(queryableSource, identityInfoSource, localStorage)
	where TSecurityContext : class, ISecurityContext
	where TSecurityContextIdent : notnull
{
	protected override SecurityContextData<TSecurityContextIdent> CreateSecurityContextData(TSecurityContext securityContext) =>

		new(this.IdentityInfo.Id.Getter(securityContext), displayService.ToString(securityContext),
			hierarchicalInfo.ParentFunc(securityContext).Maybe(IdentityInfo.Id.Getter));

	protected override IEnumerable<TSecurityContext> GetSecurityContextsWithMasterExpand(TSecurityContext startSecurityObject)
	{
		return startSecurityObject.GetAllElements(hierarchicalInfo.ParentFunc);
	}
}