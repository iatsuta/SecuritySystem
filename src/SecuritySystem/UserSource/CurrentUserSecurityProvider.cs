using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using Microsoft.Extensions.DependencyInjection;
using SecuritySystem.Providers;
using SecuritySystem.RelativeDomainPathInfo;
using SecuritySystem.SecurityAccessor;
using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class CurrentUserSecurityProvider<TDomainObject>(
    IServiceProvider serviceProvider,
    IUserPathInfo userPathInfo,
    CurrentUserSecurityProviderRelativeKey? key = null) : ISecurityProvider<TDomainObject>
{
    private readonly Lazy<ISecurityProvider<TDomainObject>> lazyInnerProvider = new(() =>
    {
        var generics = new[] { typeof(TDomainObject), userPathInfo.UserDomainObjectType };

        var relativeDomainPathInfoType = typeof(IRelativeDomainPathInfo<,>).MakeGenericType(generics);

        var relativePathKey = key?.Name;

        var relativeDomainPathInfo = relativePathKey == null
            ? serviceProvider.GetRequiredService(relativeDomainPathInfoType)
            : serviceProvider.GetRequiredKeyedService(relativeDomainPathInfoType, relativePathKey);

        return (ISecurityProvider<TDomainObject>)
            ActivatorUtilities.CreateInstance(
                serviceProvider,
                typeof(CurrentUserSecurityProvider<,>).MakeGenericType(generics),
                relativeDomainPathInfo);
    });

    private ISecurityProvider<TDomainObject> InnerProvider => this.lazyInnerProvider.Value;

    public IQueryable<TDomainObject> InjectFilter(IQueryable<TDomainObject> queryable) => this.InnerProvider.InjectFilter(queryable);

    public AccessResult GetAccessResult(TDomainObject domainObject) => this.InnerProvider.GetAccessResult(domainObject);

    public bool HasAccess(TDomainObject domainObject) => this.InnerProvider.HasAccess(domainObject);

    public SecurityAccessorData GetAccessorData(TDomainObject domainObject) => this.InnerProvider.GetAccessorData(domainObject);
}

public class CurrentUserSecurityProvider<TDomainObject, TUser>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IRelativeDomainPathInfo<TDomainObject, TUser> relativeDomainPathInfo,
    UserPathInfo<TUser> userPathInfo,
    ICurrentUser currentUser) : SecurityProvider<TDomainObject>(expressionEvaluatorStorage)
    where TUser : class
{
    public override Expression<Func<TDomainObject, bool>> SecurityFilter { get; } =

        relativeDomainPathInfo.CreateCondition(userPathInfo.IdPath.Select(userId => userId == currentUser.Id));

    public override SecurityAccessorData GetAccessorData(TDomainObject domainObject)
    {
        var users = relativeDomainPathInfo.GetRelativeObjects(domainObject);

        return SecurityAccessorData.Return(users.Select(user => this.ExpressionEvaluator.Evaluate(userPathInfo.NamePath, user)));
    }
}