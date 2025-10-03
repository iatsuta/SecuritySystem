namespace SecuritySystem.DiTests.DomainObjects;

public class BusinessUnitUndirectAncestorLink
{
    public required BusinessUnit Source { get; init; }

    public required BusinessUnit Target { get; init; }
}
