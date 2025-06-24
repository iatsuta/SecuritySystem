using SecuritySystem.DiTests.DomainObjects._Base;

namespace SecuritySystem.DiTests.DomainObjects;

public class BusinessUnitToAncestorChildView : PersistentDomainObjectBase
{
    public virtual BusinessUnit ChildOrAncestor { get; set; } = null!;

    public virtual BusinessUnit Source { get; set; } = null!;
}
