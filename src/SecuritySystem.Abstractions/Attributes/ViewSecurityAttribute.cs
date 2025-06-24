using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class ViewSecurityAttribute() : FromKeyedServicesAttribute(nameof(SecurityRule.View));
