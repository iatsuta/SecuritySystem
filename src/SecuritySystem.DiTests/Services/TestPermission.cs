namespace SecuritySystem.DiTests.Services;

public record TestPermission(SecurityRole SecurityRole, IReadOnlyDictionary<Type, IReadOnlyList<Guid>> Restrictions);
