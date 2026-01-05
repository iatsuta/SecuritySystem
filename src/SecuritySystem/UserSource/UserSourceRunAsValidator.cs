using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public class ExistUserRunAsValidator<TUser>(IMissedUserErrorSource missedUserErrorSource, IUserSource<TUser> userSource) : IRunAsValidator
{
    public async Task ValidateAsync(UserCredential value, CancellationToken cancellationToken)
    {
        var user = await userSource.TryGetUserAsync(value, cancellationToken);

        if (user is null)
        {
            throw missedUserErrorSource.GetNotFoundException(typeof(TUser), value);
        }
    }
}