namespace SecuritySystem.ExternalSystem.Management;

public record TypedPrincipalHeader(SecurityIdentity Identity, string Name, bool IsVirtual);
