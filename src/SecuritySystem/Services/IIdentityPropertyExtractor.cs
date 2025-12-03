using System.Reflection;

namespace SecuritySystem.Services;

public interface IIdentityPropertyExtractor
{
    PropertyInfo Extract(Type domainType);
}