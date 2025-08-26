using System.ComponentModel.DataAnnotations.Schema;
using ExampleWebApp.Domain._Base;

namespace ExampleWebApp.Domain;

[Table(nameof(TestObject), Schema = "app")]
public class TestObject : PersistentDomainObjectBase
{
    public virtual required BusinessUnit BusinessUnit { get; set; }

    public virtual required Location Location { get; set; }
}