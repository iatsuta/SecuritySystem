namespace ExampleApp.Domain;

public class BusinessUnitDirectAncestorLink
{
    public Guid Id { get; set; }

    public virtual required BusinessUnit Ancestor { get; init; }

    public virtual required BusinessUnit Child { get; init; }
}