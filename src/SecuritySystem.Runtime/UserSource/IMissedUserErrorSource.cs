using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public interface IMissedUserErrorSource
{
    Exception GetNotFoundException(Type userType, UserCredential userCredential);
}