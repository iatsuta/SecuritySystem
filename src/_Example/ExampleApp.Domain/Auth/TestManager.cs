using System.ComponentModel.DataAnnotations.Schema;
using ExampleWebApp.Domain._Base;

namespace ExampleWebApp.Domain.Auth;

[Table(nameof(TestManager), Schema = "auth")]
public class TestManager : PersistentDomainObjectBase
{
    public virtual required Employee Employee { get; set; }

    public virtual required BusinessUnit BusinessUnit { get; set; }

    public virtual required Location Location { get; set; }
}