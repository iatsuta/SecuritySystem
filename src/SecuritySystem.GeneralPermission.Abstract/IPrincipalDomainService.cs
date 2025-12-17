namespace SecuritySystem.GeneralPermission;

public interface IPrincipalDomainService<TPrincipal>
{
    Task<TPrincipal> GetOrCreateAsync(string name, CancellationToken cancellationToken = default);

    //Task SaveAsync(PrincipalData<TPrincipal> principalData, CancellationToken cancellationToken = default);

    Task RemoveAsync(TPrincipal principal, bool force = false, CancellationToken cancellationToken = default);
}
