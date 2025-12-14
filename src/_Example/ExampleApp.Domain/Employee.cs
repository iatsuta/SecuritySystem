namespace ExampleApp.Domain;

public class Employee
{
    public Guid Id { get; set; }

    public required string Login { get; set; }

    public virtual Employee? RunAs { get; set; }
}