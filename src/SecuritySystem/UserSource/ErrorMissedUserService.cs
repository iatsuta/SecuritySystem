using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public class ErrorMissedUserService<TUser> : IMissedUserService<TUser>
{
	public TUser GetUser(UserCredential userCredential)
	{
		throw this.GetNotFoundException(userCredential);
	}

	public IMissedUserService<User> ToSimple()
	{
		return new SimpleErrorMissedUserService(this.GetNotFoundException);
	}

	private Exception GetNotFoundException(UserCredential userCredential) =>
		new UserSourceException($"{typeof(TUser).Name} \"{userCredential}\" not found");

	private class SimpleErrorMissedUserService(Func<UserCredential, Exception> getError) : IMissedUserService<User>
	{
		public User GetUser(UserCredential userCredential)
		{
			throw getError(userCredential);
		}

		public IMissedUserService<User> ToSimple()
		{
			return this;
		}
	}
}