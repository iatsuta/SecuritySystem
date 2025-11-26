using System.Linq.Expressions;

using CommonFramework;

using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class UserSource<TUser>(IQueryableSource queryableSource, UserPathInfo<TUser> userPathInfo) : IUserSource<TUser>
    where TUser : class
{
    public TUser? TryGetUser(UserCredential userCredential) => this.GetQueryable(userCredential).SingleOrDefault();

    public TUser GetUser(UserCredential userCredential) =>
        this.TryGetUser(userCredential) ?? throw this.GetNotFoundException(userCredential);

    private IQueryable<TUser> GetQueryable(UserCredential userCredential) =>
        queryableSource
            .GetQueryable<TUser>()
            .Where(userPathInfo.Filter)
            .Where(this.GetCredentialFilter(userCredential));

    private Expression<Func<TUser, bool>> GetCredentialFilter(UserCredential userCredential)
    {
        return userCredential switch
        {
            UserCredential.NamedUserCredential { Name: var name } => userPathInfo.NamePath.Select(objName => objName == name),
            UserCredential.IdentUserCredential { Id: var id } => userPathInfo.IdPath.Select(objId => objId == id),
            _ => throw new ArgumentOutOfRangeException(nameof(userCredential))
        };
    }

    private Exception GetNotFoundException(UserCredential userCredential) =>
        new UserSourceException($"{typeof(TUser).Name} \"{userCredential}\" not found");
}
