namespace SecuritySystem.Configurator.Models;

public record FullRoleDto : EntityDto
{
    public bool IsVirtual { get; init; }

    public required IReadOnlyList<RoleContextDto> Contexts { get; init; }
}
