namespace SecuritySystem.Builders._Filter;

public record AccessorsFilterInfo<TDomainObject>(Func<TDomainObject, IEnumerable<string>> GetAccessorsFunc);
