using System.Collections.Immutable;

using CommonFramework;

using SecuritySystem.Credential;
using SecuritySystem.UserSource;

namespace SecuritySystem.ExternalSystem.Management;

public class RootPrincipalSourceService(IEnumerable<IPrincipalSourceService> principalSourceServices) : IRootPrincipalSourceService
{
    public IPrincipalSourceService ForPrincipal(Type principalType) => principalSourceServices.Single(pss => pss.PrincipalType == principalType);

    public IAsyncEnumerable<ManagedPrincipalHeader> GetPrincipalsAsync(string nameFilter, int limit)
    {
        return principalSourceServices
            .ToAsyncEnumerable()
            .SelectMany(ps => ps.GetPrincipalsAsync(nameFilter, limit))
            .GroupBy(header => header with { IsVirtual = false })
            .Select(g => g.Key with { IsVirtual = g.All(h => h.IsVirtual) })
            .OrderBy(header => header.IsVirtual)
            .ThenBy(header => header.Name)
            .Take(limit);
    }

    public async ValueTask<ManagedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken)
    {
        var request =

            from pss in principalSourceServices.ToAsyncEnumerable()

            from principal in pss.TryGetPrincipalAsync(userCredential, cancellationToken).ToAsyncEnumerable()

            where principal != null

            group principal by principal.Header with { IsVirtual = false }

            into g

            select new ManagedPrincipal(
                g.Key with { IsVirtual = g.All(p => p.Header.IsVirtual) },
                [.. g.SelectMany(p => p.Permissions)]);

        var preResult = await request.ToListAsync(cancellationToken);

        return preResult.SingleOrDefault(() => throw new UserSourceException($"More one principal {userCredential}"));
    }

    public IAsyncEnumerable<string> GetLinkedPrincipalsAsync(ImmutableHashSet<SecurityRole> securityRoles) =>
        principalSourceServices
            .ToAsyncEnumerable()
            .SelectMany(ps => ps.GetLinkedPrincipalsAsync(securityRoles))
            .Distinct();
}
