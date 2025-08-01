using System.Linq.Expressions;

namespace SecuritySystem;

public record IdentityInfo<TDomainObject, TIdent>(Expression<Func<TDomainObject, TIdent>> IdPath) : IdentityInfo<TDomainObject>
    where TIdent: notnull
{
    public Func<TDomainObject, TIdent> IdFunc { get; } = IdPath.Compile();

    public override Type IdentityType { get; } = typeof(TIdent);
}

public abstract record IdentityInfo<TDomainObject> : IdentityInfo
{
    public override Type DomainObjectType { get; } = typeof(TDomainObject);
}

public abstract record IdentityInfo
{
    public abstract Type DomainObjectType { get; }

    public abstract Type IdentityType { get; }
}