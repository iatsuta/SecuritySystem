namespace SecuritySystem.AvailableSecurity;

public class AvailableSecurityOperationSource(IAvailableSecurityRoleSource availableSecurityRoleSource)
    : IAvailableSecurityOperationSource
{
    public IAsyncEnumerable<SecurityOperation> GetAvailableSecurityOperations() =>
        availableSecurityRoleSource.GetAvailableSecurityRoles()
            .SelectMany(sr => sr.Information.Operations)
            .Distinct();
}
