namespace SecuritySystem.AvailableSecurity;

public interface IAvailableSecurityOperationSource
{
    Task<List<SecurityOperation>> GetAvailableSecurityOperations(CancellationToken cancellationToken = default);
}
