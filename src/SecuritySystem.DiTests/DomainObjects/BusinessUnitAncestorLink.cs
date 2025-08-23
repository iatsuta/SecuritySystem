namespace SecuritySystem.DiTests.DomainObjects;

public class BusinessUnitAncestorLink
{
    public BusinessUnit Ancestor { get; set; } = null!;

    public BusinessUnit Child { get; set; } = null!;
}
