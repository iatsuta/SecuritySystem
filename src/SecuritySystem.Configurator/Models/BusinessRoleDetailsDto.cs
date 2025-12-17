namespace SecuritySystem.Configurator.Models;

public class BusinessRoleDetailsDto
{
    public required List<OperationDto> Operations { get; set; }

    public required List<string> Principals { get; set; }
}
