using System.ComponentModel.DataAnnotations;

namespace ExampleWebApp.Domain._Base;

public abstract class PersistentDomainObjectBase
{
    [Key]
    public Guid Id { get; set; }
}