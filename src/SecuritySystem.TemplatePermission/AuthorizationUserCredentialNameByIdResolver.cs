

using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.TemplatePermission;

public class AuthorizationUserCredentialNameByIdResolver(IQueryableSource queryableSource)
    : IUserCredentialNameByIdResolver
{
    public string? TryGetUserName(Guid id)
    {
        return this.GetQueryable(id).Select(principal => (string?)principal.Name).SingleOrDefault();
    }

    private IQueryable<TPrincipal> GetQueryable(Guid id) =>
        queryableSource.GetQueryable<TPrincipal>().Where(principal => principal.Id == id);
}
