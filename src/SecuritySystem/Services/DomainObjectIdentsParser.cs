using System.Collections.Concurrent;
using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.IdentitySource;

namespace SecuritySystem.Services;

public class DomainObjectIdentsParser(IIdentityInfoSource identityInfoSource) : IDomainObjectIdentsParser
{
	private readonly ConcurrentDictionary<Type, Func<IEnumerable<string>, Array>> funcCache = new();

	public Array Parse(Type domainObjectType, IEnumerable<string> idents)
	{
		var func = funcCache.GetOrAdd(domainObjectType, _ =>
		{
			var identityInfo = identityInfoSource.GetIdentityInfo(domainObjectType);

			return (Func<IEnumerable<string>, Array>)new Func<Expression<Func<IEnumerable<string>, string[]>>>(GetParseExpression<string>)
				.CreateGenericMethod(identityInfo.IdentityType)
				.Invoke<LambdaExpression>(null)
				.Compile();
		});

		return func(idents);
	}

	private static Expression<Func<IEnumerable<string>, TIdent[]>> GetParseExpression<TIdent>()
		where TIdent : IParsable<TIdent>
	{
		return idents => Parse<TIdent>(idents);
	}

	private static TIdent[] Parse<TIdent>(IEnumerable<string> idents)
		where TIdent : IParsable<TIdent>
	{
		return idents.Select(v => TIdent.Parse(v, null)).ToArray();
	}
}