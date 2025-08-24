using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem;

public record IdentityInfo<TDomainObject, TIdent>(Expression<Func<TDomainObject, TIdent>> IdPath) : IdentityInfo<TDomainObject>
    where TIdent: notnull
{
    public Func<TDomainObject, TIdent> IdFunc { get; } = IdPath.Compile();

    public override Type IdentityType { get; } = typeof(TIdent);

    public Expression<Func<TDomainObject, bool>> CreateContainsFilter(TIdent[] idents)
    {
        return this.IdPath.Select(ident => idents.Contains(ident));
    }

    public override Expression<Func<TDomainObject, bool>> CreateBaseContainsFilter(Array idents)
    {
        return this.CreateContainsFilter((TIdent[])idents);
    }
}

public abstract record IdentityInfo<TDomainObject> : IdentityInfo
{
    public override Type DomainObjectType { get; } = typeof(TDomainObject);

    public abstract Expression<Func<TDomainObject, bool>> CreateBaseContainsFilter(Array idents);
}

public abstract record IdentityInfo
{
    public abstract Type DomainObjectType { get; }

    public abstract Type IdentityType { get; }
}