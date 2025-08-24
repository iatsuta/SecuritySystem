namespace SecuritySystem.DiTests.DomainObjects;

public class BusinessUnitToAncestorChildView
{
    public virtual BusinessUnit ChildOrAncestor { get; set; } = null!;

    public virtual BusinessUnit Source { get; set; } = null!;
}
