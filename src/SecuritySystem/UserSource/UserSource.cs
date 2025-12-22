using GenericQueryable;

using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public class UserSource<TUser>(IUserQueryableSource<TUser> userQueryableSource, IMissedUserService<TUser> missedUserService) : IUserSource<TUser>
	where TUser : class
{
	private readonly Dictionary<UserCredential, TUser?> tryGetUserCache = new();

	private readonly Dictionary<UserCredential, TUser> getUserCache = new();

	private IUserSource<User>? simpleCache;


	public Type UserType { get; } = typeof(TUser);

    public async Task<TUser?> TryGetUserAsync(UserCredential userCredential, CancellationToken cancellationToken)
	{
		if (!tryGetUserCache.TryGetValue(userCredential, out var result))
		{
			tryGetUserCache[userCredential] = result = await userQueryableSource.GetQueryable(userCredential).GenericSingleOrDefaultAsync(cancellationToken);
		}

		return result;
	}

	public async Task<TUser> GetUserAsync(UserCredential userCredential, CancellationToken cancellationToken)
	{
		if (!getUserCache.TryGetValue(userCredential, out var result))
		{
			getUserCache[userCredential] = result = await this.TryGetUserAsync(userCredential, cancellationToken) ?? missedUserService.GetUser(userCredential);
		}

		return result;
	}

    public IUserSource<User> ToSimple()
	{
		return this.simpleCache ??= new UserSource<User>(userQueryableSource.ToSimple(), missedUserService.ToSimple());
	}
}