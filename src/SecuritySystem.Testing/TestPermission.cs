namespace SecuritySystem.Testing;

public record TestPermission(SecurityRole SecurityRole)
{
    public IReadOnlyDictionary<Type, Array> Restrictions { get; init; } = new Dictionary<Type, Array>();

    public (DateTime StartDate, DateTime? EndDate) Period { get; init; } = (DateTime.MinValue, null);

    public static implicit operator TestPermission(SecurityRole securityRole)
    {
        return new TestPermission(securityRole);
    }
}