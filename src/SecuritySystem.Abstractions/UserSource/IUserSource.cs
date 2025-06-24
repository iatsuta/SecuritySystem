using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public interface IUserSource<out TUser> : IUserSource
{
    new TUser? TryGetUser(UserCredential userCredential);

    new TUser GetUser(UserCredential userCredential);
}

public interface IUserSource
{
    User? TryGetUser(UserCredential userCredential);

    User GetUser(UserCredential userCredential);
}
