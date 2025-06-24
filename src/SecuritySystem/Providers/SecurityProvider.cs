using System.Linq.Expressions;

using CommonFramework;

using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.SecurityAccessor;

namespace SecuritySystem.Providers
{
    public abstract class SecurityProvider<TDomainObject> : ISecurityProvider<TDomainObject>
    {
        private readonly Lazy<Func<TDomainObject, bool>> lazyHasAccessFunc;


        protected SecurityProvider(IExpressionEvaluator expressionEvaluator) => this.lazyHasAccessFunc = LazyHelper.Create(() => expressionEvaluator.Compile(this.SecurityFilter));
        
        public abstract Expression<Func<TDomainObject, bool>> SecurityFilter { get; }
        
        public virtual IQueryable<TDomainObject> InjectFilter(IQueryable<TDomainObject> queryable) => queryable.Where(this.SecurityFilter);

        public virtual bool HasAccess(TDomainObject domainObject) => this.lazyHasAccessFunc.Value(domainObject);

        public abstract SecurityAccessorData GetAccessorData(TDomainObject domainObject);
    }
}
