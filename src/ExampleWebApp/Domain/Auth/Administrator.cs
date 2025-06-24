using System.ComponentModel.DataAnnotations.Schema;

using ExampleWebApp.Domain._Base;

namespace ExampleWebApp.Domain.Auth;

[Table(nameof(Administrator), Schema = "auth")]
public class Administrator : PersistentDomainObjectBase
{
    public Employee Employee { get; set; } = null!;
}