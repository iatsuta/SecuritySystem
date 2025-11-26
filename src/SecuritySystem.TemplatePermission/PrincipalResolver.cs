using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.TemplatePermission;

public class PrincipalResolver<TPrincipal>(IQueryableSource queryableSource, IIdentityInfoSource identityInfoSource) : IPrincipalResolver<TPrincipal>
	where TPrincipal : class
{
    private readonly IQueryable<TPrincipal> principalQueryable = queryableSource.GetQueryable<TPrincipal>();

    private readonly IdentityInfo<TPrincipal, Guid> principalIdentityInfo = identityInfoSource.GetIdentityInfo<TPrincipal, Guid>();

	public async Task<TPrincipal> Resolve(UserCredential userCredential, CancellationToken cancellationToken)
    {
		switch (userCredential)
        {
            case UserCredential.IdentUserCredential { Id: var id }:
                return await queryable.Single(v => v.) principalRepository.LoadAsync(id, cancellationToken);

            case UserCredential.NamedUserCredential { Name: var name }:
                return await principalRepository.GetQueryable().GenericSingleAsync(principal => principal.Name == name, cancellationToken);

            default:
                throw new ArgumentOutOfRangeException(nameof(userCredential));
        }
    }
}
