namespace SecuritySystem.RelativeDomainPathInfo;

public record SelfRelativeDomainPathInfo<T>() : SingleRelativeDomainPathInfo<T, T>(v => v);
