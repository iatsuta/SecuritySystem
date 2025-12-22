// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public abstract record SecurityContextInfo(TypedSecurityIdentity Identity, string Name)
{
    public abstract Type Type { get; }
}

public record SecurityContextInfo<TSecurityContext>(TypedSecurityIdentity Identity, string Name) : SecurityContextInfo(Identity, Name)
    where TSecurityContext : ISecurityContext
{
    public override Type Type { get; } = typeof(TSecurityContext);
}
