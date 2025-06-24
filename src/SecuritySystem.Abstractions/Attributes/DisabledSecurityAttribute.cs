using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class DisabledSecurityAttribute() : FromKeyedServicesAttribute(nameof(SecurityRule.Disabled));
