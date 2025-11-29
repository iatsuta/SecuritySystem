using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public interface IUserSource<out TUser>
{
    TUser GetUser(UserCredential userCredential);

    IUserSource<User> ToSimple();
}