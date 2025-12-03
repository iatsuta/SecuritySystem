using System.Reflection;

namespace SecuritySystem.Services;

public class IdentityPropertyExtractor(IdentityPropertySourceSettings settings) : IIdentityPropertyExtractor
{
    public PropertyInfo Extract(Type domainType)
    {
        var propertyName = settings.DefaultPropertyName;

        return domainType.GetProperty(propertyName) ?? throw new Exception($"{propertyName} property in {domainType.Name} not found");
    }
}