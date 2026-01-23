using SecuritySystem.Providers;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public abstract record SecurityRule
{
    public SecurityRuleCredential? CustomCredential { get; init; }


    /// <summary>
    /// Правило доступа для просмотра объекта
    /// </summary>
    public static ModeSecurityRule View { get; } = new(nameof(View));

    /// <summary>
    /// Правило доступа для редактирования объекта
    /// </summary>
    public static ModeSecurityRule Edit { get; } = new(nameof(Edit));

    /// <summary>
    /// Правило доступа для отключения безопасности
    /// </summary>
    public static DomainSecurityRule.ProviderSecurityRule Disabled { get; } = new(typeof(ISecurityProvider<>), nameof(Disabled));


    public static implicit operator SecurityRule(SecurityOperation securityOperation) => securityOperation.ToSecurityRule();

    public static implicit operator SecurityRule(SecurityRole securityRole) => securityRole.ToSecurityRule();

    public static implicit operator SecurityRule(SecurityRole[] securityRoles) => securityRoles.ToSecurityRule();

    public static implicit operator SecurityRule(DomainSecurityRule.NonExpandedRolesSecurityRule[] securityRules) => securityRules.ToSecurityRule();

    public record ModeSecurityRule(string Name) : SecurityRule
    {
        public override string ToString() => this.Name;

        public DomainSecurityRule.DomainModeSecurityRule ToDomain<TDomainObject>() => this.ToDomain(typeof(TDomainObject));

        public DomainSecurityRule.DomainModeSecurityRule ToDomain(Type domainType) => new(domainType, this) { CustomCredential = this.CustomCredential };
    }
}