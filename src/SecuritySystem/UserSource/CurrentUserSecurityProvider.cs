using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.DependencyInjection;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;
using CommonFramework.RelativePath;
using CommonFramework.VisualIdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Providers;
using SecuritySystem.SecurityAccessor;
using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class CurrentUserSecurityProvider<TDomainObject>(
    IServiceProvider serviceProvider,
	IServiceProxyFactory serviceProxyFactory,
    IEnumerable<UserSourceInfo> userSourceInfoList,
    IIdentityInfoSource identityInfoSource,
    CurrentUserSecurityProviderRelativeKey? key = null) : ISecurityProvider<TDomainObject>
{
    private readonly Lazy<ISecurityProvider<TDomainObject>> lazyInnerProvider = new(() =>
    {
	    var (actualUserSourceInfo, actualRelativeDomainPathInfo) =
		    TryGetActualUserSourceInfo() ?? throw new SecuritySystemException($"Can't found RelativePath for {typeof(TDomainObject)}");

	    var identityInfo = identityInfoSource.GetIdentityInfo(actualUserSourceInfo.UserType);

        return serviceProxyFactory.Create<ISecurityProvider<TDomainObject>>(
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
	IdentityInfo<TUser, TIdent> identityInfo,
	IVisualIdentityInfoSource visualIdentityInfoSource,
	ISecurityIdentityConverter<TIdent> securityIdentityConverter,
	ICurrentUserSource<TUser> currentUserSource) : SecurityProvider<TDomainObject>(expressionEvaluatorStorage)
	where TUser : class
	where TIdent : notnull
{
	private readonly Func<TUser, string> nameSelector = visualIdentityInfoSource.GetVisualIdentityInfo<TUser>().Name.Getter;

	public override Expression<Func<TDomainObject, bool>> SecurityFilter { get; } =

		relativeDomainPathInfo.CreateCondition(
			identityInfo.Id.Path.Select(
				ExpressionHelper.GetEqualityWithExpr(securityIdentityConverter.Convert(currentUserSource.ToSimple().CurrentUser.Identity).Id)));

	public override SecurityAccessorData GetAccessorData(TDomainObject domainObject)
	{
		var users = relativeDomainPathInfo.GetRelativeObjects(domainObject);

		return SecurityAccessorData.Return(users.Select(nameSelector));
	}
}