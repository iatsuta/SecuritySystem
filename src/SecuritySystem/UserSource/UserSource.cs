using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public class UserSource<TUser>(IUserQueryableSource<TUser> userQueryableSource, IMissedUserService<TUser> missedUserService) : IUserSource<TUser>
	where TUser : class
{
	public TUser? TryGetUser(UserCredential userCredential) => userQueryableSource.GetQueryable(userCredential).SingleOrDefault();

	public TUser GetUser(UserCredential userCredential) => this.TryGetUser(userCredential) ?? missedUserService.GetUser(userCredential);

	public IUserSource<User> ToSimple()
	{
		return new UserSource<User>(userQueryableSource.ToSimple(), missedUserService.ToSimple());
	}
}