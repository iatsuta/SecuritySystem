namespace SecuritySystem.Services;

public interface IIdentsParser<out TIdent> : IIdentsParser
{
	new TIdent[] Parse(IEnumerable<string> idents);

	Array IIdentsParser.Parse(IEnumerable<string> idents) => this.Parse(idents);
}

public interface IIdentsParser
{
	Array Parse(IEnumerable<string> idents);
}