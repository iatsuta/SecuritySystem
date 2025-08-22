using SecuritySystem.Services;

namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class PlainTypedSecurityContextStorage<TSecurityContext, TIdent>(
    IQueryableSource queryableSource,
    LocalStorage<TSecurityContext, TIdent> localStorage,
    IdentityInfo<TSecurityContext, TIdent> identityInfo,
    ISecurityContextDisplayService<TSecurityContext> displayService)
    : TypedSecurityContextStorageBase<TSecurityContext, TIdent>(queryableSource, localStorage, identityInfo)
    where TSecurityContext : class, ISecurityContext
    where TIdent : notnull
{
    protected override SecurityContextData<TIdent> CreateSecurityContextData(TSecurityContext securityContext) =>

        new(identityInfo.IdFunc(securityContext), displayService.ToString(securityContext), default);

    protected override IEnumerable<TSecurityContext> GetSecurityContextsWithMasterExpand(TSecurityContext startSecurityObject)
    {
        return [startSecurityObject];
    }
}
