namespace SecuritySystem.HierarchicalExpand;

public interface IRealTypeResolver
{
    Type Resolve(Type identity);
}