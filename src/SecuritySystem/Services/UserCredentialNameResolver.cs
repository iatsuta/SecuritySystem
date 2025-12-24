using CommonFramework;

using SecuritySystem.Credential;
using SecuritySystem.UserSource;

namespace SecuritySystem.Services;

public class UserCredentialNameResolver(IEnumerable<IUserSource> userSourceList) : IUserCredentialNameResolver
{
    public virtual string GetUserName(UserCredential userCredential)
    {
        switch (userCredential)
        {
            case UserCredential.NamedUserCredential { Name: var name }:
                return name;

            case UserCredential.IdentUserCredential { Identity: var identity }:
            {
                var request =

                    from userSource in userSourceList

                    let user = userSource.ToSimple().TryGetUser(userCredential)

                    where user != null

                    select user.Name;

                return request.Distinct().Single(
                    () => new Exception($"{nameof(UserCredential)} with id {identity} not found"),
                    names => new Exception($"More one {nameof(UserCredential)} with id {identity}: {names.Join(", ", name => $"\"{name}\"")}"));
            }

            default: throw new ArgumentOutOfRangeException(nameof(userCredential));
        }
    }
}