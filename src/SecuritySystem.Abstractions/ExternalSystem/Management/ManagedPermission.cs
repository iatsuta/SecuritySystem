namespace SecuritySystem.ExternalSystem.Management;

public record ManagedPermission : ManagedPermissionData
{
    public required SecurityIdentity Identity { get; init; }

    public required bool IsVirtual { get; init; }
}