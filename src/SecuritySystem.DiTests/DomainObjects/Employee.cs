using SecuritySystem.DiTests.DomainObjects._Base;

namespace SecuritySystem.DiTests.DomainObjects;

public class Employee : PersistentDomainObjectBase
{
    public BusinessUnit BusinessUnit { get; set; }

    public BusinessUnit AltBusinessUnit { get; set; }

    public Location Location { get; set; }

    public bool TestCheckbox { get; set; }
}
