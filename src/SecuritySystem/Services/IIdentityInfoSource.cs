namespace SecuritySystem.Services;

public interface IIdentityInfoSource
{
    IdentityInfo? TryGetIdentityInfo(Type domainObjectType);

    IdentityInfo GetIdentityInfo(Type domainObjectType) =>
        this.TryGetIdentityInfo(domainObjectType) ?? throw new Exception($"{nameof(IdentityInfo)} for {domainObjectType.Name} not found");
}