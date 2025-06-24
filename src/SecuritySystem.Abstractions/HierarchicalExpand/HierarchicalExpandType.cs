namespace SecuritySystem.HierarchicalExpand;

/// <summary>
/// The direction of expansion for hierarchical objects.
/// </summary>
[Flags]
public enum HierarchicalExpandType
{
    /// <summary>
    /// Only the initial object is loaded.
    /// </summary>
    None = 0,

    /// <summary>
    /// The initial object and all its parents are loaded.
    /// </summary>
    Parents = 1,

    /// <summary>
    /// The initial object and all its children are loaded.
    /// </summary>
    Children = 2,

    /// <summary>
    /// The initial object, its parents, and its children are all loaded.
    /// </summary>
    All = Parents + Children
}