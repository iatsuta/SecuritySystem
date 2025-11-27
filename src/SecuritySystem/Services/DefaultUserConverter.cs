using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.UserSource;

using System.Linq.Expressions;
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

	public Expression<Func<TUser, User>> GetConvertFunc()
	{
		return this.lazyInnerUserSource.Value.GetConvertFunc();
	}
}

public class DefaultUserConverter<TUser, TIdent>(UserSourceInfo<TUser> userSourceInfo, IdentityInfo<TUser, TIdent> identityInfo) : IDefaultUserConverter<TUser>
	where TIdent : notnull
{
	public Expression<Func<TUser, User>> GetConvertFunc()
	{
		return ExpressionEvaluateHelper.InlineEvaluate<Func<TUser, User>>(ee =>
			user => new User(ee.Evaluate(userSourceInfo.NamePath, user), new SecurityIdentity<TIdent>(ee.Evaluate(identityInfo.IdPath, user))));
	}
}