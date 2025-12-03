using System.Reflection;

namespace SecuritySystem.Services;

public interface IVisualIdentityPropertyExtractor
{
	PropertyInfo? TryExtract(Type domainType);
}