namespace SecuritySystem.Services;

public class IdentsParser<TIdent>(IFormatProviderSource formatProviderSource) : IIdentsParser<TIdent>
	where TIdent : IParsable<TIdent>
{
	public TIdent[] Parse(IEnumerable<string> idents) => idents.Select(v => TIdent.Parse(v, formatProviderSource.FormatProvider)).ToArray();
}