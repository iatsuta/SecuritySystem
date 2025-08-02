using System.Reflection;

namespace SecuritySystem.Services;

public class IdentityPropertySource : IIdentityPropertySource
{
    public PropertyInfo GetIdentityProperty(Type domainType)
    {
        var propertyName = "Id";

        return domainType.GetProperty(propertyName) ?? throw new Exception($"{propertyName} property in {domainType.Name} not found");
    }
}