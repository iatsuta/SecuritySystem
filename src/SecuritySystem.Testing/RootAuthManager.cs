using SecuritySystem.Credential;

namespace SecuritySystem.Testing;

public class RootAuthManager(IServiceProvider rootServiceProvider)
{
    public RootUserCredentialManager For(UserCredential? userCredential = null)
    {
        return new RootUserCredentialManager(rootServiceProvider, userCredential);
    }
}