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
	IDefaultUserConverter<TUser> defaultUserConverter) : IUserQueryableSource<TUser>
	where TUser : class
	where TIdent : notnull
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
		return userCredential switch
		{
			UserCredential.NamedUserCredential { Name: var name } => visualIdentityInfo.Name.Path.Select(objName => objName == name),

			UserCredential.IdentUserCredential { Identity: SecurityIdentity<TIdent> { Id: var id } } =>
				identityInfo.Id.Path.Select(ExpressionHelper.GetEqualityWithExpr(id)),

			UserCredential.IdentUserCredential => _ => false,

			_ => throw new ArgumentOutOfRangeException(nameof(userCredential))
		};
	}

	public class SimpleUserQueryableSource(Func<UserCredential, IQueryable<TUser>> getFilteredQueryable, IDefaultUserConverter<TUser> defaultUserConverter)
		: IUserQueryableSource<User>
	{
		public IQueryable<User> GetQueryable(UserCredential userCredential) =>
			getFilteredQueryable(userCredential).Select(defaultUserConverter.ConvertExpression);

		public IUserQueryableSource<User> ToSimple() => this;
	}
}