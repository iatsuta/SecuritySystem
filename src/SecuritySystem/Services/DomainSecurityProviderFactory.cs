using System.Linq.Expressions;

using CommonFramework;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.Providers;


using SecuritySystem.UserSource;

namespace SecuritySystem.Services;

public class DomainSecurityProviderFactory<TDomainObject>(
    IServiceProvider serviceProvider,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    ISecurityRuleDeepOptimizer deepOptimizer,
    IRoleBaseSecurityProviderFactory<TDomainObject> roleBaseSecurityProviderFactory) : IDomainSecurityProviderFactory<TDomainObject>
{
    public virtual ISecurityProvider<TDomainObject> Create(
        DomainSecurityRule securityRule,
        SecurityPath<TDomainObject> securityPath)
    {
        return this.CreateInternal(deepOptimizer.Optimize(securityRule), securityPath);
    }

    protected virtual ISecurityProvider<TDomainObject> CreateInternal(
        DomainSecurityRule baseSecurityRule,
        SecurityPath<TDomainObject> securityPath)
    {
        switch (baseSecurityRule)
        {
            case DomainSecurityRule.RoleBaseSecurityRule securityRule:
                return roleBaseSecurityProviderFactory.Create(securityRule, securityPath);

            case DomainSecurityRule.CurrentUserSecurityRule securityRule:
            {
                var args = new object?[]
                    {
                        securityRule.RelativePathKey == null
                            ? null
                            : new CurrentUserSecurityProviderRelativeKey(securityRule.RelativePathKey)
                    }
                    .Where(arg => arg != null)
                    .Select(arg => arg!)
                    .ToArray();

                return ActivatorUtilities.CreateInstance<CurrentUserSecurityProvider<TDomainObject>>(serviceProvider, args);
            }

            case DomainSecurityRule.ProviderSecurityRule securityRule:
            {
                var securityProviderType =
                    securityRule.GenericSecurityProviderType.MakeGenericType(typeof(TDomainObject));

                var securityProvider = securityRule.Key == null
                    ? serviceProvider.GetRequiredService(securityProviderType)
                    : serviceProvider.GetRequiredKeyedService(securityProviderType, securityRule.Key);

                return (ISecurityProvider<TDomainObject>)securityProvider;
            }

            case DomainSecurityRule.ProviderFactorySecurityRule securityRule:
            {
                var securityProviderFactoryType =
                    securityRule.GenericSecurityProviderFactoryType.MakeGenericType(typeof(TDomainObject));

                var securityProviderFactoryUntyped =
                    securityRule.Key == null
                        ? serviceProvider.GetRequiredService(securityProviderFactoryType)
                        : serviceProvider.GetRequiredKeyedService(securityProviderFactoryType, securityRule.Key);

                var securityProviderFactory = (IFactory<ISecurityProvider<TDomainObject>>)securityProviderFactoryUntyped;

                return securityProviderFactory.Create();
            }

            case DomainSecurityRule.ConditionFactorySecurityRule securityRule:
            {
                var conditionFactoryType =
                    securityRule.GenericConditionFactoryType.MakeGenericType(typeof(TDomainObject));

                var conditionFactoryUntyped = serviceProvider.GetRequiredService(conditionFactoryType);

                var conditionFactory = (IFactory<Expression<Func<TDomainObject, bool>>>)conditionFactoryUntyped;

                return new ConditionSecurityProvider<TDomainObject>(conditionFactory.Create(), expressionEvaluatorStorage);
            }

            case DomainSecurityRule.RelativeConditionSecurityRule securityRule:
            {
                var conditionInfo = securityRule.RelativeConditionInfo;

                var factoryType = typeof(RequiredRelativeConditionFactory<,>).MakeGenericType(
                    typeof(TDomainObject),
                    conditionInfo.RelativeDomainObjectType);

                var untypedConditionFactory = ActivatorUtilities.CreateInstance(serviceProvider, factoryType, conditionInfo);

                var conditionFactory = (IFactory<Expression<Func<TDomainObject, bool>>>)untypedConditionFactory;

                var condition = conditionFactory.Create();

                return new ConditionSecurityProvider<TDomainObject>(condition, expressionEvaluatorStorage);
            }

            case DomainSecurityRule.FactorySecurityRule securityRule:
            {
                var dynamicRoleFactoryUntyped = serviceProvider.GetRequiredService(securityRule.RuleFactoryType);

                var dynamicRoleFactory = (IFactory<DomainSecurityRule>)dynamicRoleFactoryUntyped;

                return this.CreateInternal(dynamicRoleFactory.Create(), securityPath);
            }

            case DomainSecurityRule.OverrideAccessDeniedMessageSecurityRule securityRule:
            {
                return this.CreateInternal(securityRule.BaseSecurityRule, securityPath)
                    .OverrideAccessDeniedResult(accessDeniedResult => accessDeniedResult with { CustomMessage = securityRule.CustomMessage });
            }

            case DomainSecurityRule.OrSecurityRule securityRule:
                return this.CreateInternal(securityRule.Left, securityPath).Or(this.CreateInternal(securityRule.Right, securityPath));

            case DomainSecurityRule.AndSecurityRule securityRule:
                return this.CreateInternal(securityRule.Left, securityPath).And(this.CreateInternal(securityRule.Right, securityPath));

            case DomainSecurityRule.NegateSecurityRule securityRule:
                return this.CreateInternal(securityRule.InnerRule, securityPath).Negate();

            case DomainSecurityRule.DomainModeSecurityRule:
            case DomainSecurityRule.SecurityRuleHeader:
            case DomainSecurityRule.ClientSecurityRule:
                throw new Exception("Must be optimized");

            default:
                throw new ArgumentOutOfRangeException(nameof(baseSecurityRule));
        }
    }
}