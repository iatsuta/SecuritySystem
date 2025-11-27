using System.Linq.Expressions;

using CommonFramework;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class UserSource<TUser>(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource) : IUserSource<TUser>
	where TUser : class
{
	private readonly Lazy<IUserSource<TUser>> lazyInnerUserSource = new(() =>
	{
		if (typeof(TUser) == typeof(User))
		{
			return (IUserSource<TUser>)ActivatorUtilities.CreateInstance(serviceProvider, typeof(UserSource));
		}
		else
		{
			var identityInfo = identityInfoSource.GetIdentityInfo(typeof(TUser));

			var innerUserSourceType = typeof(UserSource<,>).MakeGenericType(typeof(TUser), identityInfo.IdentityType);

			return (IUserSource<TUser>)ActivatorUtilities.CreateInstance(serviceProvider, innerUserSourceType, identityInfo);
		}
	});

	public TUser? TryGetUser(UserCredential userCredential)
	{
		return this.lazyInnerUserSource.Value.TryGetUser(userCredential);
	}

	public TUser GetUser(UserCredential userCredential)
	{
		return this.lazyInnerUserSource.Value.GetUser(userCredential);
	}
}

public class UserSource<TUser, TIdent>(IQueryableSource queryableSource, UserSourceInfo<TUser> userSourceInfo, IdentityInfo<TUser, TIdent> identityInfo)
	: IUserSource<TUser>
	where TUser : class
	where TIdent : notnull
{
	public TUser? TryGetUser(UserCredential userCredential) => this.GetQueryable(userCredential).SingleOrDefault();

	public TUser GetUser(UserCredential userCredential) =>
		this.TryGetUser(userCredential) ?? throw this.GetNotFoundException(userCredential);

	private IQueryable<TUser> GetQueryable(UserCredential userCredential) =>
		queryableSource
			.GetQueryable<TUser>()
			.Where(userSourceInfo.Filter)
			.Where(this.GetCredentialFilter(userCredential));

	private Expression<Func<TUser, bool>> GetCredentialFilter(UserCredential userCredential)
	{
		return userCredential switch
		{
			UserCredential.NamedUserCredential { Name: var name } => userSourceInfo.NamePath.Select(objName => objName == name),

			UserCredential.IdentUserCredential { Identity: SecurityIdentity<TIdent> { Id: var id } } =>
				identityInfo.IdPath.Select(ExpressionHelper.GetEqualityWithExpr(id)),

			_ => throw new ArgumentOutOfRangeException(nameof(userCredential))
		};
	}

	private Exception GetNotFoundException(UserCredential userCredential) =>
		new UserSourceException($"{typeof(TUser).Name} \"{userCredential}\" not found");
}

public class UserSource : IUserSource<User>
{
	public User TryGetUser(UserCredential userCredential)
	{
		throw new NotImplementedException();
	}

	public User GetUser(UserCredential userCredential)
	{
		throw new NotImplementedException();
	}
}