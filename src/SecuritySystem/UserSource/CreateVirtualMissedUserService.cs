using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class CreateVirtualMissedUserService<TUser>(IVisualIdentityInfoSource visualIdentityInfoSources) : IMissedUserService<TUser>
	where TUser : class, new()
{
	private readonly Action<TUser, string> nameSetter = visualIdentityInfoSources.GetVisualIdentityInfo<TUser>().Name.Setter;

	public TUser GetUser(UserCredential userCredential)
	{
		var user = new TUser();

		switch (userCredential)
		{
			case UserCredential.NamedUserCredential namedUserCredential:
			{
				nameSetter(user, namedUserCredential.Name);
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