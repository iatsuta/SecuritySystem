using SecuritySystem.Services;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class PlainTypedSecurityContextStorage<TSecurityContext, TSecurityContextIdent>(
    IQueryableSource queryableSource,
    LocalStorage<TSecurityContext, TSecurityContextIdent> localStorage,
    IIdentityInfoSource identityInfoSource,
    ISecurityContextDisplayService<TSecurityContext> displayService)
    : TypedSecurityContextStorageBase<TSecurityContext, TSecurityContextIdent>(queryableSource, identityInfoSource, localStorage)
    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
{
    protected override SecurityContextData<TSecurityContextIdent> CreateSecurityContextData(TSecurityContext securityContext) =>

        new(this.IdentityInfo.Id.Getter(securityContext), displayService.ToString(securityContext), default);

    protected override IEnumerable<TSecurityContext> GetSecurityContextsWithMasterExpand(TSecurityContext startSecurityObject)
    {
        return [startSecurityObject];
    }
}
