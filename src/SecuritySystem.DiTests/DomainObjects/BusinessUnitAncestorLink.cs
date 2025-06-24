using SecuritySystem.DiTests.DomainObjects._Base;

namespace SecuritySystem.DiTests.DomainObjects;

public class BusinessUnitAncestorLink : PersistentDomainObjectBase
{
    public BusinessUnit Ancestor { get; set; } = null!;

    public BusinessUnit Child { get; set; } = null!;
}
