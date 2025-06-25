using System.ComponentModel.DataAnnotations.Schema;

using ExampleWebApp.Domain._Base;

namespace ExampleWebApp.Domain.Auth;

[Table(nameof(TestManager), Schema = "auth")]
public class TestManager : PersistentDomainObjectBase
{
    public virtual Employee Employee { get; set; } = null!;

    public virtual BusinessUnit BusinessUnit { get; set; } = null!;
}