using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public class ExistsOtherwiseUsersRunAsValidator<TUser>(IEnumerable<IUserSource<TUser>> userSources, IMissedUserErrorSource missedUserErrorSource)
    : IRunAsValidator
{
    public async Task ValidateAsync(UserCredential value, CancellationToken cancellationToken)
    {
        foreach (var userSource in userSources.Where(userSource => userSource.UserType != typeof(TUser)))
        {
            var user = await userSource.TryGetUserAsync(value, cancellationToken);

            if (user is null)
            {
                throw missedUserErrorSource.GetNotFoundException(userSource.UserType, value);
            }
        }
    }
}