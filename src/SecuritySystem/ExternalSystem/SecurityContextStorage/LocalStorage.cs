﻿namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public class LocalStorage<TSecurityContext>
    where TSecurityContext : ISecurityContext
{
    private readonly HashSet<TSecurityContext> items = [];

    public bool IsExists(Guid securityEntityId)
    {
        return this.items.Select(v => v.Id).Contains(securityEntityId);
    }

    public bool Register(TSecurityContext securityContext)
    {
        return this.items.Add(securityContext);
    }
}
