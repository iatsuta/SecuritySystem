using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public interface IMissedUserService<out TUser>
{
	TUser GetUser(UserCredential userCredential);

	IMissedUserService<User> ToSimple();
}