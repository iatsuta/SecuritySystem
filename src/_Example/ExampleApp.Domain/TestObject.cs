
namespace ExampleApp.Domain;

public class TestObject
{
    public Guid Id { get; set; }

    public virtual required BusinessUnit BusinessUnit { get; set; }

    public virtual required Location Location { get; set; }
}