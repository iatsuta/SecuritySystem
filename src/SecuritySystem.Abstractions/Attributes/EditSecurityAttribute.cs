using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class EditSecurityAttribute() : FromKeyedServicesAttribute(nameof(SecurityRule.Edit));
