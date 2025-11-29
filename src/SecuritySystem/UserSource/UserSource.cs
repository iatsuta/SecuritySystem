using GenericQueryable;
using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public class UserSource<TUser>(IUserQueryableSource<TUser> userQueryableSource, IMissedUserService<TUser> missedUserService) : IUserSource<TUser>
	where TUser : class
{
	public Task<TUser?> TryGetUserAsync(UserCredential userCredential, CancellationToken cancellationToken) =>
		userQueryableSource.GetQueryable(userCredential).GenericSingleOrDefaultAsync(cancellationToken);

	public async Task<TUser> GetUserAsync(UserCredential userCredential, CancellationToken cancellationToken) =>
		await this.TryGetUserAsync(userCredential, cancellationToken) ?? missedUserService.GetUser(userCredential);

	public IUserSource<User> ToSimple()
	{
		return new UserSource<User>(userQueryableSource.ToSimple(), missedUserService.ToSimple());
	}
}