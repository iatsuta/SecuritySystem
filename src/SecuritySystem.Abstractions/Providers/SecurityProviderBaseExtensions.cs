using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;

using SecuritySystem.AccessDenied;
using SecuritySystem.SecurityAccessor;

namespace SecuritySystem.Providers;

public static class SecurityProviderBaseExtensions
{
    extension<TDomainObject>(ISecurityProvider<TDomainObject> securityProvider)
    {
        public ISecurityProvider<TDomainObject> OverrideAccessDeniedResult(Func<AccessResult.AccessDeniedResult, AccessResult.AccessDeniedResult> selector) =>
            new OverrideAccessDeniedResultSecurityProvider<TDomainObject>(securityProvider, selector);

        public ISecurityProvider<TDomainObject> And(Expression<Func<TDomainObject, bool>> securityFilter,
            IExpressionEvaluatorStorage expressionEvaluatorStorage) =>
            securityProvider.And(new ConditionSecurityProvider<TDomainObject>(securityFilter, expressionEvaluatorStorage));

        public ISecurityProvider<TDomainObject> Or(Expression<Func<TDomainObject, bool>> securityFilter,
            IExpressionEvaluatorStorage expressionEvaluatorStorage) =>
            securityProvider.Or(new ConditionSecurityProvider<TDomainObject>(securityFilter, expressionEvaluatorStorage));

        public ISecurityProvider<TDomainObject> And(ISecurityProvider<TDomainObject> otherSecurityProvider) =>
            new CompositeSecurityProvider<TDomainObject>(securityProvider, otherSecurityProvider, true);

        public ISecurityProvider<TDomainObject> Or(ISecurityProvider<TDomainObject> otherSecurityProvider) =>
            new CompositeSecurityProvider<TDomainObject>(securityProvider, otherSecurityProvider, false);

        public ISecurityProvider<TDomainObject> Negate() =>
            new NegateSecurityProvider<TDomainObject>(securityProvider);

        public ISecurityProvider<TDomainObject> Except(
            ISecurityProvider<TDomainObject> otherSecurityProvider) =>
            securityProvider.And(otherSecurityProvider.Negate());

        public void CheckAccess(
            TDomainObject domainObject,
            IAccessDeniedExceptionService accessDeniedExceptionService)
        {
            switch (securityProvider.GetAccessResult(domainObject))
            {
                case AccessResult.AccessDeniedResult accessDenied:
                    throw accessDeniedExceptionService.GetAccessDeniedException(accessDenied);

                case AccessResult.AccessGrantedResult:
                    break;

                default:
                    throw new InvalidOperationException("unknown access result");
            }
        }
    }

    extension<TDomainObject>(IEnumerable<ISecurityProvider<TDomainObject>> securityProviders)
    {
        public ISecurityProvider<TDomainObject> And() =>
            securityProviders.Match(
                () => new DisabledSecurityProvider<TDomainObject>(),
                single => single,
                many => many.Aggregate((v1, v2) => v1.And(v2)));

        public ISecurityProvider<TDomainObject> Or() =>
            securityProviders.Match(
                () => new AccessDeniedSecurityProvider<TDomainObject>(),
                single => single,
                many => many.Aggregate((v1, v2) => v1.Or(v2)));
    }

    private class CompositeSecurityProvider<TDomainObject>(
        ISecurityProvider<TDomainObject> securityProvider,
        ISecurityProvider<TDomainObject> otherSecurityProvider,
        bool orAnd)
        : ISecurityProvider<TDomainObject>
    {
        public IQueryable<TDomainObject> InjectFilter(IQueryable<TDomainObject> queryable) =>
            orAnd
                ? securityProvider.InjectFilter(queryable).Pipe(otherSecurityProvider.InjectFilter)
                : securityProvider.InjectFilter(queryable).Union(otherSecurityProvider.InjectFilter(queryable));

        public AccessResult GetAccessResult(TDomainObject domainObject) =>
            orAnd
                ? securityProvider.GetAccessResult(domainObject).And(otherSecurityProvider.GetAccessResult(domainObject))
                : securityProvider.GetAccessResult(domainObject).Or(otherSecurityProvider.GetAccessResult(domainObject));

        public bool HasAccess(TDomainObject domainObject) =>
            orAnd
                ? securityProvider.HasAccess(domainObject) && otherSecurityProvider.HasAccess(domainObject)
                : securityProvider.HasAccess(domainObject) || otherSecurityProvider.HasAccess(domainObject);

        public SecurityAccessorData GetAccessorData(TDomainObject domainObject)
        {
            var left = securityProvider.GetAccessorData(domainObject);

            var right = otherSecurityProvider.GetAccessorData(domainObject);

            return orAnd
                ? new SecurityAccessorData.AndSecurityAccessorData(left, right)
                : new SecurityAccessorData.OrSecurityAccessorData(left, right);
        }
    }

    private class NegateSecurityProvider<TDomainObject>(ISecurityProvider<TDomainObject> securityProvider)
        : ISecurityProvider<TDomainObject>
    {
        public IQueryable<TDomainObject> InjectFilter(IQueryable<TDomainObject> queryable) =>
            queryable.Except(securityProvider.InjectFilter(queryable));

        public AccessResult GetAccessResult(TDomainObject domainObject)
        {
            switch (securityProvider.GetAccessResult(domainObject))
            {
                case AccessResult.AccessDeniedResult:
                    return AccessResult.AccessGrantedResult.Default;

                case AccessResult.AccessGrantedResult:
                    return AccessResult.AccessDeniedResult.Create(domainObject);

                default:
                    throw new InvalidOperationException("unknown access result");
            }
        }

        public bool HasAccess(TDomainObject domainObject) => !securityProvider.HasAccess(domainObject);

        public SecurityAccessorData GetAccessorData(TDomainObject domainObject)
        {
            var baseResult = securityProvider.GetAccessorData(domainObject);

            return new SecurityAccessorData.NegateSecurityAccessorData(baseResult);
        }
    }
}