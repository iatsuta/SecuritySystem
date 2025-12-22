namespace ExampleApp.Domain.Auth.Virtual;

public class Administrator
{
    public Guid Id { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}