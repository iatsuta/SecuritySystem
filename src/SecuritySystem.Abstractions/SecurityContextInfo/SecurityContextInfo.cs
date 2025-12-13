// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public abstract record SecurityContextInfo(SecurityIdentity Identity, string Name)
{
    public abstract Type Type { get; }
}

public record SecurityContextInfo<TSecurityContext>(SecurityIdentity Identity, string Name) : SecurityContextInfo(Identity, Name)
    where TSecurityContext : ISecurityContext
{
    public override Type Type { get; } = typeof(TSecurityContext);
}
