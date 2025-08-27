using System.ComponentModel.DataAnnotations;

namespace ExampleApp.Domain._Base;

public abstract class PersistentDomainObjectBase
{
    [Key]
    public Guid Id { get; set; }
}