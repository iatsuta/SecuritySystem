using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ExampleApp.Domain._Base;

namespace ExampleApp.Domain;

[Table(nameof(Employee), Schema = "app")]
public class Employee : PersistentDomainObjectBase
{
    [MaxLength(255)]
    public required string Login { get; set; }

    public virtual Employee? RunAs { get; set; }
}