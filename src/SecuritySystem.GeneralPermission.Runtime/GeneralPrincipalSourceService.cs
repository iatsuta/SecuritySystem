using System.Collections.Immutable;
using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.GeneralPermission;

public class GeneralPrincipalSourceService<TPrincipal>(
    IQueryableSource queryableSource,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IAvailablePrincipalSource<TPrincipal> availablePrincipalSource,
    IManagedPrincipalHeaderConverter<TPrincipal> principalHeaderConverter,
    IManagedPrincipalConverter<TPrincipal> principalConverter,
    IUserQueryableSource<TPrincipal> userQueryableSource) : IPrincipalSourceService
    where TPrincipal : class
{
    private readonly PropertyAccessors<TPrincipal, string> nameAccessors = visualIdentityInfoSource.GetVisualIdentityInfo<TPrincipal>().Name;

    private readonly IQueryable<TPrincipal> principalQueryable = queryableSource.GetQueryable<TPrincipal>();

    public Type PrincipalType { get; } = typeof(TPrincipal);

    public IAsyncEnumerable<ManagedPrincipalHeader> GetPrincipalsAsync(string nameFilter, int limit)
    {
        return principalQueryable
            .Pipe(
                !string.IsNullOrWhiteSpace(nameFilter),
                q => q.Where(this.nameAccessors.Path.Select(principalName => principalName.Contains(nameFilter))))
            .Select(principalHeaderConverter.ConvertExpression)
            .GenericAsAsyncEnumerable();
    }

    public async ValueTask<ManagedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken)
    {
        var principal = await userQueryableSource.GetQueryable(userCredential).GenericSingleOrDefaultAsync(cancellationToken);

        if (principal is null)
        {
            return null;
        }
        else
        {
            return await principalConverter.ToManagedPrincipalAsync(principal, cancellationToken);
        }
    }

    public IAsyncEnumerable<string> GetLinkedPrincipalsAsync(ImmutableHashSet<SecurityRole> securityRoles)
    {
        return availablePrincipalSource.GetAvailablePrincipalsQueryable(
                new DomainSecurityRule.ExpandedRolesSecurityRule(securityRoles)
                {
                    CustomCredential = new SecurityRuleCredential.AnyUserCredential()
                })
            .Select(this.nameAccessors.Path)
            .Distinct()
            .GenericAsAsyncEnumerable();
    }
}