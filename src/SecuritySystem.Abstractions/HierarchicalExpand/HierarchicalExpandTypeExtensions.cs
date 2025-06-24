namespace SecuritySystem.HierarchicalExpand;

public static class HierarchicalExpandTypeExtensions
{
    public static HierarchicalExpandType Reverse(this HierarchicalExpandType hierarchicalExpandType)
    {
        return hierarchicalExpandType switch
        {
            HierarchicalExpandType.Parents => HierarchicalExpandType.Children,
            HierarchicalExpandType.Children => HierarchicalExpandType.Parents,
            _ => hierarchicalExpandType
        };
    }
}