using System.Reflection;

namespace SecuritySystem.Services;

public class IdentityPropertySource(IdentityPropertySourceSettings settings) : IIdentityPropertySource
{
    public PropertyInfo GetIdentityProperty(Type domainType)
    {
        var propertyName = settings.DefaultPropertyName;

        return domainType.GetProperty(propertyName) ?? throw new Exception($"{propertyName} property in {domainType.Name} not found");
    }
}