using ExampleWebApp.Domain._Base;

using System.ComponentModel.DataAnnotations.Schema;

namespace ExampleWebApp.Domain;

[Table(nameof(Employee), Schema = "app")]
public class Employee : PersistentDomainObjectBase
{
    public string Login { get; set; } = null!;

    public virtual Employee? RunAs { get; set; }
}