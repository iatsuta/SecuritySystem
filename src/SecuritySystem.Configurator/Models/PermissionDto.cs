namespace SecuritySystem.Configurator.Models;

public record PermissionDto
{
    public required string Id { get; init; }

    public required string Role { get; init; }

    public required string RoleId { get; init; }

    public required string Comment { get; init; }

    public required IReadOnlyList<ContextDto> Contexts { get; init; }

    public required DateTime StartDate { get; init; }

    public required DateTime? EndDate { get; init; }

    public required bool IsVirtual { get; init; }
}
