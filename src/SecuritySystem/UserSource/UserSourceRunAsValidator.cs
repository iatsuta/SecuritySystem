using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class UserSourceRunAsValidator<TUser>(IUserSource<TUser> userSource) : IRunAsValidator
{
    public async Task ValidateAsync(UserCredential value, CancellationToken cancellationToken) =>
		_ = await userSource.GetUserAsync(value, cancellationToken);
}
