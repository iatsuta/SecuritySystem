using ExampleWebApp.Domain._Base;

using System.ComponentModel.DataAnnotations.Schema;

using SecuritySystem;

namespace ExampleWebApp.Domain;

[Table(nameof(BusinessUnit), Schema = "app")]
public class BusinessUnit : PersistentDomainObjectBase, ISecurityContext
{
    public string Name { get; set; } = null!;
}