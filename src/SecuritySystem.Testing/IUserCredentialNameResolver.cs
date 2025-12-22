using SecuritySystem.Credential;

namespace SecuritySystem.Testing;

public interface IUserCredentialNameResolver
{
    string GetUserName(UserCredential userCredential);
}