using CommonFramework;

using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class UserSourceCredentialNameByIdResolver<TUser>(IQueryableSource queryableSource, UserPathInfo<TUser> userPathInfo) : IUserCredentialNameByIdResolver
    where TUser : class
{
    public string? TryGetUserName(Guid id)
    {
        return this.GetQueryable(id).Select(userPathInfo.NamePath).Select(v => (string?)v).SingleOrDefault();
    }

    private IQueryable<TUser> GetQueryable(Guid id) =>
        queryableSource
            .GetQueryable<TUser>()
            .Where(userPathInfo.Filter)
            .Where(userPathInfo.IdPath.Select(objId => objId == id));
}