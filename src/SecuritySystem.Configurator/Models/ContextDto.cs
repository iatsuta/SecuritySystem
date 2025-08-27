namespace SecuritySystem.Configurator.Models;

public class ContextDto : EntityDto
{
    public List<RestrictionDto> Entities { get; set; }
}
