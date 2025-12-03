using System.Reflection;

namespace SecuritySystem.Services;

public class VisualIdentityPropertyExtractor(VisualIdentityPropertySourceSettings settings) : IVisualIdentityPropertyExtractor
{
	public PropertyInfo? TryExtract(Type domainType)
	{
		var request =

			from propertyName in settings.DefaultPropertyNames

			let property = domainType.GetProperty(propertyName)

			where property != null && property.PropertyType == typeof(string)

			select property;

		return request.FirstOrDefault();
	}
}