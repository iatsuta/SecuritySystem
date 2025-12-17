using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.IdentitySource;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Services;

public class SecurityIdentityFilterFactory<TDomainObject>(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource)
	: ISecurityIdentityFilterFactory<TDomainObject>
{
	private readonly Lazy<ISecurityIdentityFilterFactory<TDomainObject>> lazyInnerService = new(() =>
	{
		var identityInfo = identityInfoSource.GetIdentityInfo(typeof(TDomainObject));

		var innerServiceType = typeof(SecurityIdentityFilterFactory<,>).MakeGenericType(typeof(TDomainObject), identityInfo.IdentityType);

		return (ISecurityIdentityFilterFactory<TDomainObject>)ActivatorUtilities.CreateInstance(serviceProvider, innerServiceType, identityInfo);
	});

	public Expression<Func<TDomainObject, bool>> CreateFilter(SecurityIdentity securityIdentity) =>
		this.lazyInnerService.Value.CreateFilter(securityIdentity);
}

public class SecurityIdentityFilterFactory<TDomainObject, TIdent>(
	IdentityInfo<TDomainObject, TIdent> identityInfo,
	ISecurityIdentityConverter<TIdent> securityIdentityConverter) : ISecurityIdentityFilterFactory<TDomainObject>
	where TDomainObject : class
	where TIdent : notnull
{
	public Expression<Func<TDomainObject, bool>> CreateFilter(SecurityIdentity securityIdentity)
	{
		return identityInfo.Id.Path.Select(ExpressionHelper.GetEqualityWithExpr(securityIdentityConverter.Convert(securityIdentity).Id));
	}
}