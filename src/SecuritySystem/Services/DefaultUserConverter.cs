using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.UserSource;

using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

namespace SecuritySystem.Services;

public class DefaultUserConverter<TUser>(
	IServiceProvider serviceProvider,
	IIdentityInfoSource identityInfoSource,
	IVisualIdentityInfoSource visualIdentityInfoSource) : IDefaultUserConverter<TUser>
{
	private readonly Lazy<IDefaultUserConverter<TUser>> lazyInnerUserSource = new(() =>
	{
		var identityInfo = identityInfoSource.GetIdentityInfo(typeof(TUser));

		var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TUser>();

		var innerUserSourceType = typeof(DefaultUserConverter<,>).MakeGenericType(typeof(TUser), identityInfo.IdentityType);

		return (IDefaultUserConverter<TUser>)ActivatorUtilities.CreateInstance(serviceProvider, innerUserSourceType, identityInfo, visualIdentityInfo);
	});

	private IDefaultUserConverter<TUser> InnerUserSource => lazyInnerUserSource.Value;

	public Expression<Func<TUser, User>> ConvertExpression => this.InnerUserSource.ConvertExpression;

	public Func<TUser, User> ConvertFunc => this.InnerUserSource.ConvertFunc;
}

public class DefaultUserConverter<TUser, TIdent>(
	IdentityInfo<TUser, TIdent> identityInfo,
	VisualIdentityInfo<TUser> visualIdentityInfo) : IDefaultUserConverter<TUser>
	where TIdent : notnull
{
	private readonly Tuple<Expression<Func<TUser, User>>, Func<TUser, User>> convertData = FuncHelper.Create(() =>
    {
        var convertExpr = ExpressionEvaluateHelper.InlineEvaluate<Func<TUser, User>>(ee =>
            user => new User(ee.Evaluate(visualIdentityInfo.Name.Path, user), TypedSecurityIdentity.Create(ee.Evaluate(identityInfo.Id.Path, user))));

		return new Tuple<Expression<Func<TUser, User>>, Func<TUser, User>>(convertExpr, convertExpr.Compile());
	}).Invoke();

	public Expression<Func<TUser, User>> ConvertExpression => convertData.Item1;

	public Func<TUser, User> ConvertFunc => convertData.Item2;
}