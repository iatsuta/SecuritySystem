namespace SecuritySystem.ExternalSystem.Management;

public record ManagedPrincipal(ManagedPrincipalHeader Header, IReadOnlyList<ManagedPermission> Permissions);
