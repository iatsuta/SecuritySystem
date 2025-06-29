﻿namespace SecuritySystem.ExternalSystem.Management;

public record TypedPrincipal(TypedPrincipalHeader Header, IReadOnlyList<TypedPermission> Permissions);
