using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class UserSourceRunAsValidator<TUser>(IUserSource<TUser> userSource) : IRunAsValidator
{
    public void Validate(UserCredential userCredential) => _ = userSource.GetUser(userCredential);
}
