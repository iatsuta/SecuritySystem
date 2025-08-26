using System.ComponentModel.DataAnnotations;
using ExampleWebApp.Domain._Base;

using System.ComponentModel.DataAnnotations.Schema;

namespace ExampleWebApp.Domain;

[Table(nameof(Employee), Schema = "app")]
public class Employee : PersistentDomainObjectBase
{
    [MaxLength(255)]
    public required string Login { get; set; }

    public virtual Employee? RunAs { get; set; }
}