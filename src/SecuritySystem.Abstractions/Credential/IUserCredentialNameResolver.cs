namespace SecuritySystem.Credential;

public interface IUserCredentialNameResolver
{
    string GetUserName(UserCredential userCredential);
}
