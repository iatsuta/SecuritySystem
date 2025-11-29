using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.UserSource;

using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;

namespace SecuritySystem.Services;

public class DefaultUserConverter<TUser>(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource) : IDefaultUserConverter<TUser>
{
	private readonly Lazy<IDefaultUserConverter<TUser>> lazyInnerUserSource = new(() =>
	{
		var identityInfo = identityInfoSource.GetIdentityInfo(typeof(TUser));

		var innerUserSourceType = typeof(DefaultUserConverter<,>).MakeGenericType(typeof(TUser), identityInfo.IdentityType);

		return (IDefaultUserConverter<TUser>)ActivatorUtilities.CreateInstance(serviceProvider, innerUserSourceType, identityInfo);
	});

	private IDefaultUserConverter<TUser> InnerUserSource => lazyInnerUserSource.Value;

	public Expression<Func<TUser, User>> ConvertExpression => this.InnerUserSource.ConvertExpression;

	public Func<TUser, User> ConvertFunc => this.InnerUserSource.ConvertFunc;
}

public class DefaultUserConverter<TUser, TIdent>(UserSourceInfo<TUser> userSourceInfo, IdentityInfo<TUser, TIdent> identityInfo) : IDefaultUserConverter<TUser>
	where TIdent : notnull
{
	private readonly Tuple<Expression<Func<TUser, User>>, Func<TUser, User>> convertData = FuncHelper.Create(() =>
	{
		var convertExpr = ExpressionEvaluateHelper.InlineEvaluate<Func<TUser, User>>(ee =>
			user => new User(ee.Evaluate(userSourceInfo.Name.Path, user), new SecurityIdentity<TIdent>(ee.Evaluate(identityInfo.Id.Path, user))));

		return new Tuple<Expression<Func<TUser, User>>, Func<TUser, User>>(convertExpr, convertExpr.Compile());
	}).Invoke();

	public Expression<Func<TUser, User>> ConvertExpression => convertData.Item1;

	public Func<TUser, User> ConvertFunc => convertData.Item2;
}