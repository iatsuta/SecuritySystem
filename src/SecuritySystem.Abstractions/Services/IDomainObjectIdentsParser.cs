namespace SecuritySystem.Services;

public interface IDomainObjectIdentsParser
{
	Array Parse(Type domainObjectType, IEnumerable<string> idents);
}