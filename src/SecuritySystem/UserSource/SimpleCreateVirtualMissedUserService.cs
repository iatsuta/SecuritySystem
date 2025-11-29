using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public class SimpleCreateVirtualMissedUserService : IMissedUserService<User>
{
	public User GetUser(UserCredential userCredential)
	{
		switch (userCredential)
		{
			case UserCredential.NamedUserCredential namedUserCredential:
			{
				return new User(namedUserCredential.Name, new SecurityIdentity<Guid>(Guid.Empty));
			}

			default:
				throw new ArgumentOutOfRangeException(nameof(userCredential));
		}
	}

	public IMissedUserService<User> ToSimple()
	{
		return this;
	}
}