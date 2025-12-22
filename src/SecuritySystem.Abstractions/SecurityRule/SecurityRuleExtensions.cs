using System.Linq.Expressions;

using CommonFramework;

using HierarchicalExpand;

using static SecuritySystem.DomainSecurityRule;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public static class SecurityRuleExtensions
{
    extension(DomainSecurityRule securityRule)
    {
        public DomainSecurityRule Or(DomainSecurityRule otherSecurityRule) =>
            new OrSecurityRule(securityRule, otherSecurityRule);

        public DomainSecurityRule And(DomainSecurityRule otherSecurityRule) =>
            new AndSecurityRule(securityRule, otherSecurityRule);

        public DomainSecurityRule Or<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityRule.Or(new RelativeConditionSecurityRule(condition.ToInfo()));

        public DomainSecurityRule And<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityRule.And(new RelativeConditionSecurityRule(condition.ToInfo()));

        public DomainSecurityRule Except<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityRule.And(condition.Not());

        public DomainSecurityRule Negate() =>
            new NegateSecurityRule(securityRule);

        public DomainSecurityRule Except(DomainSecurityRule otherSecurityRule) =>
            securityRule.And(otherSecurityRule.Negate());
    }

    extension(SecurityRole securityRole)
    {
        public DomainSecurityRule Or(DomainSecurityRule otherSecurityRule) =>
            securityRole.ToSecurityRule().Or(otherSecurityRule);

        public DomainSecurityRule And(DomainSecurityRule otherSecurityRule) =>
            securityRole.ToSecurityRule().And(otherSecurityRule);

        public DomainSecurityRule Or<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityRole.ToSecurityRule().Or(condition);

        public DomainSecurityRule And<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityRole.ToSecurityRule().And(condition);

        public DomainSecurityRule Except<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityRole.ToSecurityRule().Except(condition);

        public DomainSecurityRule Negate() => securityRole.ToSecurityRule().Negate();

        public DomainSecurityRule Except(DomainSecurityRule otherSecurityRule) =>
            securityRole.ToSecurityRule().Except(otherSecurityRule);

        public NonExpandedRolesSecurityRule ToSecurityRule(
            HierarchicalExpandType? customExpandType = null,
            SecurityRuleCredential? customCredential = null,
            SecurityPathRestriction? customRestriction = null) =>
            new[] { securityRole }.ToSecurityRule(customExpandType, customCredential, customRestriction);
    }

    extension(SecurityOperation securityOperation)
    {
        public DomainSecurityRule Or(DomainSecurityRule otherSecurityRule) =>
            securityOperation.ToSecurityRule().Or(otherSecurityRule);

        public DomainSecurityRule And(DomainSecurityRule otherSecurityRule) =>
            securityOperation.ToSecurityRule().And(otherSecurityRule);

        public DomainSecurityRule Or<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityOperation.ToSecurityRule().Or(condition);

        public DomainSecurityRule And<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityOperation.ToSecurityRule().And(condition);

        public DomainSecurityRule Except<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityOperation.ToSecurityRule().Except(condition);

        public DomainSecurityRule Negate() =>
            securityOperation.ToSecurityRule().Negate();

        public DomainSecurityRule Except(DomainSecurityRule otherSecurityRule) =>
            securityOperation.ToSecurityRule().Except(otherSecurityRule);

        public OperationSecurityRule ToSecurityRule(
            HierarchicalExpandType? customExpandType = null,
            SecurityRuleCredential? customCredential = null,
            SecurityPathRestriction? customRestriction = null) =>
            new(securityOperation)
            {
                CustomExpandType = customExpandType,
                CustomCredential = customCredential,
                CustomRestriction = customRestriction
            };
    }

    extension(IEnumerable<SecurityRole> securityRoles)
    {
        public DomainSecurityRule Or(DomainSecurityRule otherSecurityRule) =>
            securityRoles.ToSecurityRule().Or(otherSecurityRule);

        public DomainSecurityRule And(DomainSecurityRule otherSecurityRule) =>
            securityRoles.ToSecurityRule().And(otherSecurityRule);

        public DomainSecurityRule Or<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityRoles.ToSecurityRule().Or(condition);

        public DomainSecurityRule And<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityRoles.ToSecurityRule().And(condition);

        public DomainSecurityRule Except<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> condition) =>
            securityRoles.ToSecurityRule().Except(condition);

        public DomainSecurityRule Negate() =>
            securityRoles.ToSecurityRule().Negate();

        public DomainSecurityRule Except(DomainSecurityRule otherSecurityRule) =>
            securityRoles.ToSecurityRule().Except(otherSecurityRule);

        public NonExpandedRolesSecurityRule ToSecurityRule(
            HierarchicalExpandType? customExpandType = null,
            SecurityRuleCredential? customCredential = null,
            SecurityPathRestriction? customRestriction = null) =>
            new(
                securityRoles.OrderBy(sr => sr.Name).ToArray())
            {
                CustomExpandType = customExpandType,
                CustomCredential = customCredential,
                CustomRestriction = customRestriction
            };
    }

    extension<TSecurityRule>(TSecurityRule securityRule)
        where TSecurityRule : RoleBaseSecurityRule
    {
        public TSecurityRule TryApplyCredential(SecurityRuleCredential credential) =>
            securityRule.CustomCredential == null ? securityRule with { CustomCredential = credential } : securityRule;

        public TSecurityRule WithoutRunAs() =>
            securityRule with { CustomCredential = new SecurityRuleCredential.CurrentUserWithoutRunAsCredential() };

        public TSecurityRule TryApplyCustoms(HierarchicalExpandType? customExpandType = null,
            SecurityRuleCredential? customCredential = null,
            SecurityPathRestriction? customRestriction = null) =>

            customExpandType is null && customCredential is null && customRestriction is null
                ? securityRule
                : securityRule with
                {
                    CustomExpandType = securityRule.CustomExpandType ?? customExpandType,
                    CustomCredential = securityRule.CustomCredential ?? customCredential,
                    CustomRestriction = securityRule.CustomRestriction ?? customRestriction,
                };

        public TSecurityRule TryApplyCustoms(IRoleBaseSecurityRuleCustomData customSource) =>
            securityRule.TryApplyCustoms(customSource.CustomExpandType, customSource.CustomCredential, customSource.CustomRestriction);
    }

    public static RoleBaseSecurityRule ToSecurityRule(
        this IEnumerable<RoleBaseSecurityRule> securityRules,
        HierarchicalExpandType? customExpandType = null,
        SecurityRuleCredential? customCredential = null,
        SecurityPathRestriction? customRestriction = null)
    {
        var cache = securityRules.ToList();

        if (cache.Count == 1)
        {
            return cache.Single().TryApplyCustoms(customExpandType, customCredential, customRestriction);
        }
        else
        {
            return new RoleGroupSecurityRule(cache.ToArray())
            {
                CustomExpandType = customExpandType,
                CustomCredential = customCredential,
                CustomRestriction = customRestriction
            };
        }
    }

    public static DomainSecurityRule WithOverrideAccessDeniedMessage(
        this DomainSecurityRule securityRule,
        string customMessage) =>
        new OverrideAccessDeniedMessageSecurityRule(securityRule, customMessage);
}