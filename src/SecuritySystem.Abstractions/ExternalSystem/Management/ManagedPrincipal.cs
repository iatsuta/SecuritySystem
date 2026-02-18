using System.Collections.Immutable;

namespace SecuritySystem.ExternalSystem.Management;

public record ManagedPrincipal(ManagedPrincipalHeader Header, ImmutableArray<ManagedPermission> Permissions);
