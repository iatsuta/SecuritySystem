using System.Linq.Expressions;

namespace SecuritySystem;

public record VisualIdentityInfo<TDomainObject>(PropertyAccessors<TDomainObject, string> Name) : VisualIdentityInfo
{
	public VisualIdentityInfo(Expression<Func<TDomainObject, string>> namePath) :
		this(new PropertyAccessors<TDomainObject, string>(namePath))
	{
	}

	public override Type DomainObjectType { get; } = typeof(TDomainObject);
}

public abstract record VisualIdentityInfo
{
	public abstract Type DomainObjectType { get; }
}