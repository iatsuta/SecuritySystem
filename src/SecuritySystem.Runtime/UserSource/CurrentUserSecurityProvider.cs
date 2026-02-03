using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;
using CommonFramework.RelativePath;
using CommonFramework.VisualIdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Credential;
using SecuritySystem.Providers;
using SecuritySystem.SecurityAccessor;
using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class CurrentUserSecurityProvider<TDomainObject>(
    IServiceProvider serviceProvider,
	IServiceProxyFactory serviceProxyFactory,
    IEnumerable<UserSourceInfo> userSourceInfoList,
    IIdentityInfoSource identityInfoSource,
    SecurityRuleCredential? securityRuleCredential = null,
    CurrentUserSecurityProviderRelativeKey? key = null) : ISecurityProvider<TDomainObject>
{
    private readonly Lazy<ISecurityProvider<TDomainObject>> lazyInnerService = new(() =>
    {
	    var (actualUserSourceInfo, actualRelativeDomainPathInfo) =
		    TryGetActualUserSourceInfo() ?? throw new SecuritySystemException($"Can't found RelativePath for {typeof(TDomainObject)}");

	    var identityInfo = identityInfoSource.GetIdentityInfo(actualUserSourceInfo.UserType);

        var innerServiceType =
            typeof(CurrentUserSecurityProvider<,,>).MakeGenericType(typeof(TDomainObject), actualUserSourceInfo.UserType, identityInfo.IdentityType);

        var innerServiceArgs = new[]
            {
                actualRelativeDomainPathInfo,
                identityInfo,
                securityRuleCredential
            }.Where(arg => arg != null)
            .Select(arg => arg!)
            .ToArray();

        return serviceProxyFactory.Create<ISecurityProvider<TDomainObject>>(innerServiceType, innerServiceArgs);

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

	private ISecurityProvider<TDomainObject> InnerService => this.lazyInnerService.Value;

    public IQueryable<TDomainObject> InjectFilter(IQueryable<TDomainObject> queryable) => this.InnerService.InjectFilter(queryable);

    public AccessResult GetAccessResult(TDomainObject domainObject) => this.InnerService.GetAccessResult(domainObject);

    public bool HasAccess(TDomainObject domainObject) => this.InnerService.HasAccess(domainObject);

    public SecurityAccessorData GetAccessorData(TDomainObject domainObject) => this.InnerService.GetAccessorData(domainObject);
}

public class CurrentUserSecurityProvider<TDomainObject, TUser, TIdent>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IRelativeDomainPathInfo<TDomainObject, TUser> relativeDomainPathInfo,
    IdentityInfo<TUser, TIdent> identityInfo,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    ISecurityIdentityConverter<TIdent> securityIdentityConverter,
    IUserNameResolver<TUser> userNameResolver,
    IUserSource<TUser> userSource,
    SecurityRuleCredential? baseSecurityRuleCredential = null) : SecurityProvider<TDomainObject>(expressionEvaluatorStorage)
    where TUser : class
    where TIdent : notnull
{
    private readonly Func<TUser, string> nameSelector = visualIdentityInfoSource.GetVisualIdentityInfo<TUser>().Name.Getter;

    public override Expression<Func<TDomainObject, bool>> SecurityFilter { get; } =

        (baseSecurityRuleCredential ?? new SecurityRuleCredential.CurrentUserWithRunAsCredential())
        .Pipe(securityRuleCredential =>
        {
            var userName = userNameResolver.Resolve(securityRuleCredential);

            if (userName == null)
            {
                return _ => true;
            }
            else
            {
                var userId = securityIdentityConverter.Convert(userSource.ToSimple().GetUser(userName).Identity).Id;

                return identityInfo.Id.Path.Select(ExpressionHelper.GetEqualityWithExpr(userId));
            }
        })
        .Pipe(relativeDomainPathInfo.CreateCondition);

    public override SecurityAccessorData GetAccessorData(TDomainObject domainObject)
    {
        var users = relativeDomainPathInfo.GetRelativeObjects(domainObject);

        return SecurityAccessorData.Return(users.Select(nameSelector));
    }
}