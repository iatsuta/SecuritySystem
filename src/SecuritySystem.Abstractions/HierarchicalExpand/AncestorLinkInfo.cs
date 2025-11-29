using System.Linq.Expressions;

namespace SecuritySystem.HierarchicalExpand;

public record AncestorLinkInfo<TDomainObject, TAncestorLink>(
	PropertyAccessors<TAncestorLink, TDomainObject> From,
	PropertyAccessors<TAncestorLink, TDomainObject> To)
{
	public AncestorLinkInfo(
		Expression<Func<TAncestorLink, TDomainObject>> fromPath,
		Expression<Func<TAncestorLink, TDomainObject>> toPath)
		: this(new PropertyAccessors<TAncestorLink, TDomainObject>(fromPath), new PropertyAccessors<TAncestorLink, TDomainObject>(toPath))
	{
	}

	public AncestorLinkInfo<TDomainObject, TAncestorLink> Reverse() => new(this.To.Path, this.From.Path);
}