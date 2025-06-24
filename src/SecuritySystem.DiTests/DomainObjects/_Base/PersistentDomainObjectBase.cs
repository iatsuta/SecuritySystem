namespace SecuritySystem.DiTests.DomainObjects._Base;

public class PersistentDomainObjectBase : IIdentityObject<Guid>
{
    public Guid Id { get; set; }
}
