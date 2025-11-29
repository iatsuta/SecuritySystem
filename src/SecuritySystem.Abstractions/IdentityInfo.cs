using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem;

public record IdentityInfo<TDomainObject, TIdent>(PropertyAccessors<TDomainObject, TIdent> Id) : IdentityInfo<TDomainObject>
    where TIdent : notnull
{
	public IdentityInfo(Expression<Func<TDomainObject, TIdent>> idPath) :
		this(new PropertyAccessors<TDomainObject, TIdent>(idPath))
	{
	}

	public override Type IdentityType { get; } = typeof(TIdent);

    public Expression<Func<TDomainObject, bool>> CreateContainsFilter(IEnumerable<TIdent> idents)
    {
        return this.Id.Path.Select(ident => idents.Contains(ident));
    }

    public override object GetId(TDomainObject domainObject)
    {
	    return this.Id.Getter(domainObject);
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