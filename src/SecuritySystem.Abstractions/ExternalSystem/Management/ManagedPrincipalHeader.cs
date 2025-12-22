namespace SecuritySystem.ExternalSystem.Management;

public record ManagedPrincipalHeader(SecurityIdentity Identity, string Name, bool IsVirtual);
