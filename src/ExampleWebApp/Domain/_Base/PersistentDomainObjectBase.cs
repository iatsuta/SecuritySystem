using System.ComponentModel.DataAnnotations;

using SecuritySystem;

namespace ExampleWebApp.Domain._Base;

public abstract class PersistentDomainObjectBase : IIdentityObject<Guid>
{
    [Key]
    public Guid Id { get; set; }
}