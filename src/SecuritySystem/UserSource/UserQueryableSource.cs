using CommonFramework;

using SecuritySystem.Credential;
using SecuritySystem.Services;

using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.UserSource;

public class UserQueryableSource<TUser>(
	IServiceProvider serviceProvider,
	IIdentityInfoSource identityInfoSource,
	IVisualIdentityInfoSource visualIdentityInfoSource) : IUserQueryableSource<TUser>
{
	private readonly Lazy<IUserQueryableSource<TUser>> lazyInnerUserQueryableSource = new(() =>
	{
		var identityInfo = identityInfoSource.GetIdentityInfo(typeof(TUser));

		var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TUser>();

		return (IUserQueryableSource<TUser>)
			ActivatorUtilities.CreateInstance(
				serviceProvider,
				typeof(UserQueryableSource<,>).MakeGenericType(typeof(TUser), identityInfo.IdentityType), identityInfo, visualIdentityInfo);
	});

	public IQueryable<TUser> GetQueryable(UserCredential userCredential) => this.lazyInnerUserQueryableSource.Value.GetQueryable(userCredential);

	public IUserQueryableSource<User> ToSimple() => this.lazyInnerUserQueryableSource.Value.ToSimple();
}

public class UserQueryableSource<TUser, TIdent>(
	IQueryableSource queryableSource,
	UserSourceInfo<TUser> userSourceInfo,
	IdentityInfo<TUser, TIdent> identityInfo,
	VisualIdentityInfo<TUser> visualIdentityInfo,
	ISecurityIdentityConverter<TIdent> identityConverter,
	IDefaultUserConverter<TUser> defaultUserConverter) : IUserQueryableSource<TUser>
	where TUser : class
	where TIdent : IParsable<TIdent>
{
	public IQueryable<TUser> GetQueryable(UserCredential userCredential)
	{
		return queryableSource
			.GetQueryable<TUser>()
			.Where(userSourceInfo.FilterPath)
			.Where(this.GetCredentialFilter(userCredential));
	}

	public IUserQueryableSource<User> ToSimple()
	{
		return new SimpleUserQueryableSource(this.GetQueryable, defaultUserConverter);
	}

	private Expression<Func<TUser, bool>> GetCredentialFilter(UserCredential userCredential)
	{
		switch (userCredential)
		{
			case UserCredential.NamedUserCredential { Name: var name }:
				return visualIdentityInfo.Name.Path.Select(objName => objName == name);

			case UserCredential.IdentUserCredential { Identity: var identity }:
			{
				var convertedIdentity = identityConverter.TryConvert(identity);

				if (convertedIdentity == null)
				{
					return _ => false;
				}
				else
				{
					return identityInfo.Id.Path.Select(ExpressionHelper.GetEqualityWithExpr(convertedIdentity.Id));
				}
			}

			case UserCredential.UntypedIdentUserCredential { Id: var rawId } when TIdent.TryParse(rawId, null, out var id):
				return identityInfo.Id.Path.Select(ExpressionHelper.GetEqualityWithExpr(id));

			default:
				throw new ArgumentOutOfRangeException(nameof(userCredential));
		}
	}

	public class SimpleUserQueryableSource(Func<UserCredential, IQueryable<TUser>> getFilteredQueryable, IDefaultUserConverter<TUser> defaultUserConverter)
		: IUserQueryableSource<User>
	{
		public IQueryable<User> GetQueryable(UserCredential userCredential) =>
			getFilteredQueryable(userCredential).Select(defaultUserConverter.ConvertExpression);

		public IUserQueryableSource<User> ToSimple() => this;
	}
}