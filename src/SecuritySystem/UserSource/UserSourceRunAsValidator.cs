using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public class ExistUserRunAsValidator<TUser>(IUserSource<TUser> userSource) : IRunAsValidator
{
    public async Task ValidateAsync(UserCredential value, CancellationToken cancellationToken) =>
		_ = await userSource.GetUserAsync(value, cancellationToken);
}
