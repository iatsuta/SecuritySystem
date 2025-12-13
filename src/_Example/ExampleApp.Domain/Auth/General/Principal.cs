namespace ExampleApp.Domain.Auth.General;

public class Principal
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public virtual Principal? RunAs { get; set; }
}