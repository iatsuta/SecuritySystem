namespace SecuritySystem.Builders._Filter;

public record SecurityFilterInfo<TDomainObject>(
    Func<IQueryable<TDomainObject>, IQueryable<TDomainObject>> InjectFunc,
    Func<TDomainObject, bool> HasAccessFunc);
