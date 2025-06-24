using System.ComponentModel.DataAnnotations.Schema;

using ExampleWebApp.Domain._Base;

namespace ExampleWebApp.Domain.Auth;

[Table(nameof(TestManager), Schema = "auth")]
public class TestManager : PersistentDomainObjectBase
{
    public Employee Employee { get; set; } = null!;

    public BusinessUnit BusinessUnit { get; set; } = null!;
}