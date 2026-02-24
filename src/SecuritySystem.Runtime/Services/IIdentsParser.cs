namespace SecuritySystem.Services;

public interface IIdentsParser<in TSource, out TIdent> : IIdentsParser<TSource>
{
    new TIdent[] Parse(IEnumerable<TSource> idents);

    Array IIdentsParser<TSource>.Parse(IEnumerable<TSource> idents) => this.Parse(idents);
}

public interface IIdentsParser<in TSource>
{
    Array Parse(IEnumerable<TSource> idents);
}