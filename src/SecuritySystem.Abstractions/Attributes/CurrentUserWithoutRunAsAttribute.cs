using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Attributes;

public class CurrentUserWithoutRunAsAttribute() : FromKeyedServicesAttribute(nameof(SecurityRuleCredential.CurrentUserWithoutRunAsCredential));
