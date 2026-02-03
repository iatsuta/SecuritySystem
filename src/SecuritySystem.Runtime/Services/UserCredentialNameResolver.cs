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
                    () => new SecuritySystemException($"{nameof(UserCredential)} with id {identity.GetId()} not found"),
                    names => new SecuritySystemException($"More one {nameof(UserCredential)} with id {identity.GetId()}: {names.Join(", ", name => $"\"{name}\"")}"));
            }

            default: throw new ArgumentOutOfRangeException(nameof(userCredential));
        }
    }
}