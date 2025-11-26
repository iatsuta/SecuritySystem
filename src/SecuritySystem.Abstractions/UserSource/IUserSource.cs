using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public interface IUserSource<out TUser>
{
    TUser? TryGetUser(UserCredential userCredential);

    TUser GetUser(UserCredential userCredential);
}