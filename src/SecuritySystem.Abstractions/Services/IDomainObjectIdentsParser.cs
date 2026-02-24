namespace SecuritySystem.Services;

public interface IDomainObjectIdentsParser<in TSource>
{
	Array Parse(Type domainObjectType, IEnumerable<TSource> idents);
}