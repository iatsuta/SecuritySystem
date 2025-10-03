namespace SecuritySystem.DiTests.DomainObjects;

public class BusinessUnitDirectAncestorLink
{
    public required BusinessUnit Ancestor { get; init; }

    public required BusinessUnit Child { get; init; }
}
