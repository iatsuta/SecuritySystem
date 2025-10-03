namespace ExampleApp.Domain.Auth;

public class TestManager
{
    public Guid Id { get; set; }

    public virtual required Employee Employee { get; set; }

    public virtual required BusinessUnit BusinessUnit { get; set; }

    public virtual required Location Location { get; set; }
}