using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public class MissedUserErrorSource : IMissedUserErrorSource
{
    public Exception GetNotFoundException(Type userType, UserCredential userCredential)
    {
        return new UserSourceException($"{userType.Name} \"{userCredential}\" not found");
    }
}