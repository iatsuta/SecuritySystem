using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem;

public record IdentityInfo<TDomainObject, TIdent>(Expression<Func<TDomainObject, TIdent>> IdPath) : IdentityInfo<TDomainObject>
    where TIdent : notnull
{
	public PropertyAccessors<TDomainObject, TIdent> Accessors { get; } = new(IdPath);

	public override Type IdentityType { get; } = typeof(TIdent);

    public Expression<Func<TDomainObject, bool>> CreateContainsFilter(IEnumerable<TIdent> idents)
    {
        return this.IdPath.Select(ident => idents.Contains(ident));
    }

    public override object GetId(TDomainObject domainObject)
    {
	    return this.Accessors.Getter(domainObject);
    }
}

public abstract record IdentityInfo<TDomainObject> : IdentityInfo
{
    public override Type DomainObjectType { get; } = typeof(TDomainObject);

    public abstract object GetId(TDomainObject domainObject);
}

public abstract record IdentityInfo
{
    public abstract Type DomainObjectType { get; }

    public abstract Type IdentityType { get; }
}