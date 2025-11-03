using System.Linq.Expressions;

namespace SecuritySystem.HierarchicalExpand;

public record HierarchicalInfo<TDomainObject>(Expression<Func<TDomainObject, TDomainObject?>> ParentPath)
{
    public Func<TDomainObject, TDomainObject?> ParentFunc { get; } = ParentPath.Compile();
}