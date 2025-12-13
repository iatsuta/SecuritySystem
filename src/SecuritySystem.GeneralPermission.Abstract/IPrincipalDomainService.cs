namespace SecuritySystem.GeneralPermission;

public interface IPrincipalDomainService<TPrincipal>
{
    Task<TPrincipal> GetOrCreateAsync(string name, CancellationToken cancellationToken = default);

    Task SaveAsync(TPrincipal principal, CancellationToken cancellationToken = default);

    Task RemoveAsync(TPrincipal principal, bool force = false, CancellationToken cancellationToken = default);

    Task ValidateAsync(TPrincipal principal, CancellationToken cancellationToken = default);
}
