using SecuritySystem.Builders._Filter;

namespace SecuritySystem.Builders._Factory;

public interface IAccessorsFilterFactory<TDomainObject> : IFilterFactory<TDomainObject, AccessorsFilterInfo<TDomainObject>>;
