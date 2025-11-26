using SecuritySystem.TemplatePermission.Validation;
using SecuritySystem.Attributes;
using SecuritySystem.UserSource;

namespace SecuritySystem.TemplatePermission;

public class PrincipalDomainService(
    [DisabledSecurity] IRepository<TPrincipal> principalRepository,
    IPrincipalGeneralValidator principalGeneralValidator,
    IUserSource? userSource = null) : IPrincipalDomainService
{
    public async Task<TPrincipal> GetOrCreateAsync(string name, CancellationToken cancellationToken)
    {
        var principal = principalRepository.GetQueryable().SingleOrDefault(p => p.Name == name);

        if (principal is null)
        {
            principal = new TPrincipal { Name = name };

            var user = userSource?.TryGetUser(name);

            if (user == null)
            {
                await principalRepository.SaveAsync(principal, cancellationToken);
            }
            else
            {
                await principalRepository.InsertAsync(principal, user.Id, cancellationToken);
            }
        }

        return principal;
    }

    public async Task SaveAsync(TPrincipal principal, CancellationToken cancellationToken = default)
    {
        await this.ValidateAsync(principal, cancellationToken);

        await principalRepository.SaveAsync(principal, cancellationToken);
    }

    public async Task RemoveAsync(TPrincipal principal, bool force, CancellationToken cancellationToken)
    {
        if (force)
        {
            principal.Permissions.Foreach(p => p.DelegatedTo.Foreach(delP => delP.TPrincipal.RemoveDetail(delP)));
        }
        else if (principal.Permissions.Any())
        {
            throw new BusinessLogicException($"Removing principal \"{principal.Name}\" must be empty");
        }


        await principalRepository.RemoveAsync(principal, cancellationToken);
    }

    public async Task ValidateAsync(TPrincipal principal, CancellationToken cancellationToken)
    {
        await principalGeneralValidator.ValidateAndThrowAsync(principal, cancellationToken);
    }
}
