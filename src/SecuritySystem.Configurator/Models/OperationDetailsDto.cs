namespace SecuritySystem.Configurator.Models;

public class OperationDetailsDto
{
    public required List<string> BusinessRoles { get; set; }

    public required List<string> Principals { get; set; }
}
