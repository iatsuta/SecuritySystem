using SecuritySystem.Credential;

namespace SecuritySystem.Services;

public interface IRunAsValidator
{
    void Validate(UserCredential userCredential);
}
