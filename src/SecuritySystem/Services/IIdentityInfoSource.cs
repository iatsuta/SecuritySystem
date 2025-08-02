namespace SecuritySystem.Services;

public interface IIdentityInfoSource
{
    IdentityInfo GetIdentityInfo(Type domainType);
}

public interface IIdentityInfoSource<TIdent>
    where TIdent : notnull
{
    IdentityInfo<TDomainObject, TIdent> GetIdentityInfo<TDomainObject>();
}