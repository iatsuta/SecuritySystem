using SecuritySystem.Services;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class PlainTypedSecurityContextStorage<TSecurityContext, TIdent>(
    IQueryableSource queryableSource,
    LocalStorage<TSecurityContext, TIdent> localStorage,
    IIdentityInfoSource identityInfoSource,
    ISecurityContextDisplayService<TSecurityContext> displayService)
    : TypedSecurityContextStorageBase<TSecurityContext, TIdent>(queryableSource, identityInfoSource, localStorage)
    where TSecurityContext : class, ISecurityContext
    where TIdent : notnull
{
    protected override SecurityContextData<TIdent> CreateSecurityContextData(TSecurityContext securityContext) =>

        new(this.IdentityInfo.Id.Getter(securityContext), displayService.ToString(securityContext), default);

    protected override IEnumerable<TSecurityContext> GetSecurityContextsWithMasterExpand(TSecurityContext startSecurityObject)
    {
        return [startSecurityObject];
    }
}
