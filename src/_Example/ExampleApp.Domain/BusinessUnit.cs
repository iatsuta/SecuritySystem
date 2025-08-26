using ExampleWebApp.Domain._Base;

using SecuritySystem;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExampleWebApp.Domain;

[Table(nameof(BusinessUnit), Schema = "app")]
public class BusinessUnit : PersistentDomainObjectBase, ISecurityContext
{
    [MaxLength(255)]
    public required string Name { get; set; }
}