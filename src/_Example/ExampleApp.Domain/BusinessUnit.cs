using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ExampleApp.Domain._Base;

using SecuritySystem;

namespace ExampleApp.Domain;

[Table(nameof(BusinessUnit), Schema = "app")]
public class BusinessUnit : PersistentDomainObjectBase, ISecurityContext
{
    [MaxLength(255)]
    public required string Name { get; set; }
}