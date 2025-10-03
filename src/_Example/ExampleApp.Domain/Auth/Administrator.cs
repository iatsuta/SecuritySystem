namespace ExampleApp.Domain.Auth;

public class Administrator
{ 
    public Guid Id { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}