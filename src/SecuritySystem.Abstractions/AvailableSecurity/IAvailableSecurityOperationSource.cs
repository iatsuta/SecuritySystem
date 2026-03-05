namespace SecuritySystem.AvailableSecurity;

public interface IAvailableSecurityOperationSource
{
    IAsyncEnumerable<SecurityOperation> GetAvailableSecurityOperations();
}