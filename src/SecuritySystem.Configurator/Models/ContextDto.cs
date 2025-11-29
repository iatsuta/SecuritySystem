namespace SecuritySystem.Configurator.Models;

public record ContextDto : EntityDto
{
    public required List<RestrictionDto> Entities { get; init; }
}
