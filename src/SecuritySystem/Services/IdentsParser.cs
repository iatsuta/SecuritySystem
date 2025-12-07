namespace SecuritySystem.Services;

public class IdentsParser<TIdent> : IIdentsParser<TIdent>
	where TIdent : IParsable<TIdent>
{
	public TIdent[] Parse(IEnumerable<string> idents) => idents.Select(v => TIdent.Parse(v, null)).ToArray();
}