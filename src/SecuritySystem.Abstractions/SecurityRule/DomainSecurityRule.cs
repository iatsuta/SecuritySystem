using CommonFramework;

using HierarchicalExpand;

using SecuritySystem.Providers;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public abstract record DomainSecurityRule : SecurityRule
{
    /// <summary>
    /// Правило доступа для доменных объектов привязанных к текущему пользователю
    /// </summary>
    public static CurrentUserSecurityRule CurrentUser { get; } = new();

    /// <summary>
    /// Правило доступа для блокирования доступа
    /// </summary>
    public static ProviderSecurityRule AccessDenied { get; } = new(typeof(ISecurityProvider<>), nameof(AccessDenied));

    /// <summary>
    /// Любая роль
    /// </summary>
    public static AnyRoleSecurityRule AnyRole { get; } = new();


    public static implicit operator DomainSecurityRule(SecurityOperation securityOperation) => securityOperation.ToSecurityRule();

    public static implicit operator DomainSecurityRule(SecurityRole securityRole) => securityRole.ToSecurityRule();

    public static implicit operator DomainSecurityRule(SecurityRole[] securityRoles) => securityRoles.ToSecurityRule();

    public static implicit operator DomainSecurityRule(NonExpandedRolesSecurityRule[] securityRules) => securityRules.ToSecurityRule();

    public record SecurityRuleHeader(string Name) : DomainSecurityRule
    {
        public override string ToString() => this.Name;
    }

    public record ClientSecurityRule(string Name) : DomainSecurityRule
    {
        public override string ToString() => this.Name;
    }

    public record DomainModeSecurityRule(Type DomainType, ModeSecurityRule Mode) : DomainSecurityRule
    {
        public override string ToString() => $"{this.Mode} ({this.DomainType.Name})";
    }

    public record CurrentUserSecurityRule(string? RelativePathKey = null) : DomainSecurityRule
    {
        public override string ToString() => this.RelativePathKey ?? nameof(CurrentUser);
    }

    public record ProviderSecurityRule(Type GenericSecurityProviderType, string? Key = null) : DomainSecurityRule
    {
        public override string ToString() => this.Key ?? base.ToString();
    }

    public record ProviderFactorySecurityRule(Type GenericSecurityProviderFactoryType, string? Key = null) : DomainSecurityRule
    {
        public override string ToString() => this.Key ?? base.ToString();
    }

    public record ConditionFactorySecurityRule(Type GenericConditionFactoryType) : DomainSecurityRule;

    public record RelativeConditionSecurityRule(RelativeConditionInfo RelativeConditionInfo) : DomainSecurityRule;

    public record FactorySecurityRule(Type RuleFactoryType) : DomainSecurityRule;

    public record OverrideAccessDeniedMessageSecurityRule(DomainSecurityRule BaseSecurityRule, string CustomMessage) : DomainSecurityRule;

    public abstract record RoleBaseSecurityRule : DomainSecurityRule, IRoleBaseSecurityRuleCustomData
    {
        /// <summary>
        /// Тип разворачивания деревьев (как правило для просмотра самого дерева выбирается HierarchicalExpandType.All)
        /// </summary>
        public HierarchicalExpandType? CustomExpandType { get; init; } = null;

        public SecurityPathRestriction? CustomRestriction { get; init; } = null;

        public HierarchicalExpandType GetSafeExpandType () => this.CustomExpandType ?? HierarchicalExpandType.Children;

        public IEnumerable<SecurityContextRestriction> GetSafeSecurityContextRestrictions() =>
            (this.CustomRestriction?.SecurityContextRestrictions).EmptyIfNull();

        public IEnumerable<SecurityContextRestrictionFilterInfo> GetSafeSecurityContextRestrictionFilters() =>
            from securityContextRestriction in this.GetSafeSecurityContextRestrictions()
            where securityContextRestriction.RawFilter != null
            select securityContextRestriction.RawFilter;

        public bool EqualsCustoms(RoleBaseSecurityRule other)
        {
            return this.CustomExpandType == other.CustomExpandType
                   && this.CustomCredential == other.CustomCredential
                   && this.CustomRestriction == other.CustomRestriction;
        }

        public bool HasDefaultCustoms()
        {
            return this.CustomExpandType is null && this.CustomCredential is null && this.CustomRestriction is null;
        }

        public static implicit operator RoleBaseSecurityRule(SecurityOperation securityOperation) => securityOperation.ToSecurityRule();

        public static implicit operator RoleBaseSecurityRule(SecurityRole securityRole) => securityRole.ToSecurityRule();

        public static implicit operator RoleBaseSecurityRule(SecurityRole[] securityRoles) => securityRoles.ToSecurityRule();

        public static implicit operator RoleBaseSecurityRule(NonExpandedRolesSecurityRule[] securityRules) => securityRules.ToSecurityRule();
    }

    public interface IRoleBaseSecurityRuleCustomData
    {
        public HierarchicalExpandType? CustomExpandType { get; }

        public SecurityRuleCredential? CustomCredential { get; }

        public SecurityPathRestriction? CustomRestriction { get; }
    }

    public record AnyRoleSecurityRule : RoleBaseSecurityRule;

    public record RoleFactorySecurityRule(Type RoleFactoryType) : RoleBaseSecurityRule;

    public record NonExpandedRoleGroupSecurityRule(DeepEqualsCollection<NonExpandedRolesSecurityRule> Children) : RoleBaseSecurityRule
    {
        public NonExpandedRoleGroupSecurityRule(IEnumerable<NonExpandedRolesSecurityRule> children)
            : this(children.ToArray())
        {
        }

        public override string ToString() => this.Children.Count == 1
            ? this.Children.Single().ToString()
            : $"[{this.Children.Join(", ", sr => sr.ToString())}]";
    }

    public record ExpandedRoleGroupSecurityRule(DeepEqualsCollection<ExpandedRolesSecurityRule> Children) : RoleBaseSecurityRule
    {
        public ExpandedRoleGroupSecurityRule(IEnumerable<ExpandedRolesSecurityRule> children)
            : this(children.ToArray())
        {
        }

        public IEnumerable<ExpandedRolesSecurityRule> GetActualChildren() =>
            this.HasDefaultCustoms() ? this.Children : this.Children.Select(c => c.ApplyCustoms(this));

        public override string ToString() => this.Children.Count == 1
            ? this.Children.Single().ToString()
            : $"[{this.Children.Join(", ", sr => sr.ToString())}]";
    }

    public record OperationSecurityRule(SecurityOperation SecurityOperation) : RoleBaseSecurityRule
    {
        public override string ToString() => this.SecurityOperation.Name;

        public static implicit operator OperationSecurityRule(SecurityOperation securityOperation) => securityOperation.ToSecurityRule();
    }

    /// <summary>
    /// Список ролей ДО разворачиния дерева ролей вверх
    /// </summary>
    /// <param name="SecurityRoles">Список неразвёрнутых ролей</param>
    public record NonExpandedRolesSecurityRule(DeepEqualsCollection<SecurityRole> SecurityRoles) : RoleBaseSecurityRule
    {
        public override string ToString() => this.SecurityRoles.Count == 1
                                                 ? this.SecurityRoles.Single().Name
                                                 : $"[{this.SecurityRoles.Join(", ", sr => sr.Name)}]";

        public static implicit operator NonExpandedRolesSecurityRule(SecurityRole securityRole) => securityRole.ToSecurityRule();

        public static implicit operator NonExpandedRolesSecurityRule(SecurityRole[] securityRoles) => securityRoles.ToSecurityRule();

        public static RoleBaseSecurityRule operator +(NonExpandedRolesSecurityRule rule1, NonExpandedRolesSecurityRule rule2)
        {
            if (!rule1.EqualsCustoms(rule2))
            {
                return new NonExpandedRoleGroupSecurityRule([rule1, rule2]);
            }
            else
            {
                return rule1 with { SecurityRoles = rule1.SecurityRoles.Union(rule2.SecurityRoles).ToArray() };
            }
        }
    }

    /// <summary>
    /// Список ролей ПОСЛЕ разворачиния дерева ролей вверх
    /// </summary>
    /// <param name="SecurityRoles">Список развёрнутых ролей</param>
    public record ExpandedRolesSecurityRule(DeepEqualsCollection<SecurityRole> SecurityRoles) : RoleBaseSecurityRule
    {
        public ExpandedRolesSecurityRule(IEnumerable<SecurityRole> securityRoles)
            :this(securityRoles.ToArray())
        {
        }

        public static ExpandedRolesSecurityRule Empty { get; } = new([]);

        public override string ToString() => this.SecurityRoles.Count == 1
                                                 ? this.SecurityRoles.Single().Name
                                                 : $"[{this.SecurityRoles.Join(", ", sr => sr.Name)}]";

        public static RoleBaseSecurityRule operator +(ExpandedRolesSecurityRule rule1, ExpandedRolesSecurityRule rule2)
        {
            if (!rule1.EqualsCustoms(rule2))
            {
                return new ExpandedRoleGroupSecurityRule([rule1, rule2]);
            }
            else
            {
                return rule1 with { SecurityRoles = rule1.SecurityRoles.Union(rule2.SecurityRoles).ToArray() };
            }
        }
    }

    public record AndSecurityRule(DomainSecurityRule Left, DomainSecurityRule Right) : DomainSecurityRule;

    public record OrSecurityRule(DomainSecurityRule Left, DomainSecurityRule Right) : DomainSecurityRule;

    public record NegateSecurityRule(DomainSecurityRule InnerRule) : DomainSecurityRule;
}
