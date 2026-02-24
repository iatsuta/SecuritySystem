using CommonFramework;

namespace SecuritySystem.Services;

public class IdentsParser<TSource, TIdent>(IServiceProxyFactory serviceProxyFactory) : IIdentsParser<TSource, TIdent>
{
    private readonly Lazy<IIdentsParser<TSource, TIdent>> lazyInnerService = new(() =>
    {
        if (typeof(TSource) == typeof(TIdent))
        {
            return serviceProxyFactory.Create<IIdentsParser<TSource, TIdent>>(typeof(IdentityIdentsParser<TIdent>));
        }
        else if (typeof(TSource) == typeof(string))
        {
            return serviceProxyFactory.Create<IIdentsParser<TSource, TIdent>>(typeof(StringIdentsParser<>).MakeGenericType(typeof(TIdent)));
        }
        else
        {
            throw new InvalidOperationException();
        }
    });

    public TIdent[] Parse(IEnumerable<TSource> idents) => this.lazyInnerService.Value.Parse(idents);
}

public class StringIdentsParser<TIdent>(IFormatProviderSource formatProviderSource) : IIdentsParser<string, TIdent>
    where TIdent : IParsable<TIdent>
{
    public TIdent[] Parse(IEnumerable<string> idents) => idents.Select(v => TIdent.Parse(v, formatProviderSource.FormatProvider)).ToArray();
}

public class IdentityIdentsParser<TIdent> : IIdentsParser<TIdent, TIdent>
{
    public TIdent[] Parse(IEnumerable<TIdent> idents) => idents.ToArray();
}