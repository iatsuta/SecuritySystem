namespace SecuritySystem.Configurator.Models;

public record PrincipalHeaderDto : EntityDto
{
    public bool IsVirtual { get; init; }
}
