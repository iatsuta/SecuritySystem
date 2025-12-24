using SecuritySystem.Credential;

namespace SecuritySystem.Services;

public interface IUserCredentialNameResolver
{
    string GetUserName(UserCredential userCredential);
}