using System.Reflection;

namespace SecuritySystem.Services;

public interface IIdentityPropertySource
{
    PropertyInfo GetIdentityProperty(Type domainType);
}