using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;

using SecuritySystem.SecurityAccessor;

namespace SecuritySystem.Providers
{
    public abstract class SecurityProvider<TDomainObject> : ISecurityProvider<TDomainObject>
    {
        private readonly Lazy<Func<TDomainObject, bool>> lazyHasAccessFunc;

        private readonly Lazy<IExpressionEvaluator> lazyExpressionEvaluator;


        protected SecurityProvider(IExpressionEvaluatorStorage expressionEvaluatorStorage)
        {
            this.lazyExpressionEvaluator = LazyHelper.Create(() => expressionEvaluatorStorage.GetForType(this.GetType()));

            this.lazyHasAccessFunc = LazyHelper.Create(() => this.ExpressionEvaluator.Compile(this.SecurityFilter));
        }

        protected IExpressionEvaluator ExpressionEvaluator => this.lazyExpressionEvaluator.Value;


        public abstract Expression<Func<TDomainObject, bool>> SecurityFilter { get; }

        public virtual IQueryable<TDomainObject> InjectFilter(IQueryable<TDomainObject> queryable) => queryable.Where(this.SecurityFilter);

        public virtual bool HasAccess(TDomainObject domainObject) => this.lazyHasAccessFunc.Value(domainObject);

        public abstract SecurityAccessorData GetAccessorData(TDomainObject domainObject);
    }
}
