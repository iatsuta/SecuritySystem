using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public class CreateVirtualMissedUserService<TUser>(UserSourceInfo<TUser> userSourceInfo) : IMissedUserService<TUser>
	where TUser : class, new()
{
	public TUser GetUser(UserCredential userCredential)
	{
		var user = new TUser();

		switch (userCredential)
		{
			case UserCredential.NamedUserCredential namedUserCredential:
			{
				userSourceInfo.NameAccessors.Setter(user, namedUserCredential.Name);
				break;
			}

			default:
				throw new ArgumentOutOfRangeException(nameof(userCredential));
		}

		return user;
	}

	public IMissedUserService<User> ToSimple()
	{
		return new SimpleCreateVirtualMissedUserService();
	}
}