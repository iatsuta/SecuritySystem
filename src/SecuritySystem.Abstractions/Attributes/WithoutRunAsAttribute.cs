using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Attributes;

public class WithoutRunAsAttribute() : FromKeyedServicesAttribute(nameof(SecurityRuleCredential.CurrentUserWithoutRunAsCredential));
