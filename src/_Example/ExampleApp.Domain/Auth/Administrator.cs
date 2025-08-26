using System.ComponentModel.DataAnnotations.Schema;

using ExampleApp.Domain._Base;

namespace ExampleApp.Domain.Auth;

[Table(nameof(Administrator), Schema = "auth")]
public class Administrator : PersistentDomainObjectBase
{
    public virtual Employee Employee { get; set; } = null!;
}