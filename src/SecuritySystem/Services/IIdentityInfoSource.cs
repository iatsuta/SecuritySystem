namespace SecuritySystem.Services;

public interface IIdentityInfoSource
{
    IdentityInfo GetIdentityInfo(Type domainType);

    IdentityInfo<TDomainObject> GetIdentityInfo<TDomainObject>()
        => (IdentityInfo<TDomainObject>)this.GetIdentityInfo(typeof(TDomainObject));

    IdentityInfo<TDomainObject, TIdent> GetIdentityInfo<TDomainObject, TIdent>()
        where TIdent : notnull => (IdentityInfo<TDomainObject, TIdent>)this.GetIdentityInfo(typeof(TDomainObject));
}