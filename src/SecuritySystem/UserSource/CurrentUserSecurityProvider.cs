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
    IEnumerable<UserSourceInfo> userSourceInfoList,
    IIdentityInfoSource identityInfoSource,
    CurrentUserSecurityProviderRelativeKey? key = null) : ISecurityProvider<TDomainObject>
{
    private readonly Lazy<ISecurityProvider<TDomainObject>> lazyInnerProvider = new(() =>
    {
	    var (actualUserSourceInfo, actualRelativeDomainPathInfo) =
		    TryGetActualUserSourceInfo() ?? throw new SecuritySystemException($"Can't found {nameof(RelativeDomainPathInfo)} for {typeof(TDomainObject)}");

	    var identityInfo = identityInfoSource.GetIdentityInfo(actualUserSourceInfo.UserType);

		return (ISecurityProvider<TDomainObject>)
            ActivatorUtilities.CreateInstance(
                serviceProvider,
                typeof(CurrentUserSecurityProvider<,,>).MakeGenericType(typeof(TDomainObject), actualUserSourceInfo.UserType, identityInfo.IdentityType),
                actualRelativeDomainPathInfo, identityInfo);

		(UserSourceInfo, object)? TryGetActualUserSourceInfo()
		{
			foreach (var userSourceInfo in userSourceInfoList)
			{
				var relativeDomainPathInfoType = typeof(IRelativeDomainPathInfo<,>).MakeGenericType(typeof(TDomainObject), userSourceInfo.UserType);

				var relativePathKey = key?.Name;

				var relativeDomainPathInfo = relativePathKey == null
					? serviceProvider.GetService(relativeDomainPathInfoType)
					: serviceProvider.GetKeyedService(relativeDomainPathInfoType, relativePathKey);

				if (relativeDomainPathInfo != null)
				{
					return (userSourceInfo, relativeDomainPathInfo);
				}
			}

			return null;
		}
	});


	private ISecurityProvider<TDomainObject> InnerProvider => this.lazyInnerProvider.Value;

    public IQueryable<TDomainObject> InjectFilter(IQueryable<TDomainObject> queryable) => this.InnerProvider.InjectFilter(queryable);

    public AccessResult GetAccessResult(TDomainObject domainObject) => this.InnerProvider.GetAccessResult(domainObject);

    public bool HasAccess(TDomainObject domainObject) => this.InnerProvider.HasAccess(domainObject);

    public SecurityAccessorData GetAccessorData(TDomainObject domainObject) => this.InnerProvider.GetAccessorData(domainObject);
}

public class CurrentUserSecurityProvider<TDomainObject, TUser, TIdent>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IRelativeDomainPathInfo<TDomainObject, TUser> relativeDomainPathInfo,
    UserSourceInfo<TUser> userSourceInfo,
    IdentityInfo<TUser, TIdent> identityInfo,
    ICurrentUser currentUser) : SecurityProvider<TDomainObject>(expressionEvaluatorStorage)
    where TUser : class
    where TIdent : notnull
{
	public override Expression<Func<TDomainObject, bool>> SecurityFilter { get; } =

		relativeDomainPathInfo.CreateCondition(
			identityInfo.IdPath.Select(ExpressionHelper.GetEqualityWithExpr(((SecurityIdentity<TIdent>)currentUser.Identity).Id)));

    public override SecurityAccessorData GetAccessorData(TDomainObject domainObject)
    {
        var users = relativeDomainPathInfo.GetRelativeObjects(domainObject);

        return SecurityAccessorData.Return(users.Select(user => this.ExpressionEvaluator.Evaluate(userSourceInfo.NamePath, user)));
    }
}