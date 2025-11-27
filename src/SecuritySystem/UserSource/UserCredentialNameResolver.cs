using CommonFramework;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class UserCredentialNameByIdentityResolver<TUser>(
	IServiceProvider serviceProvider,
	IIdentityInfoSource identityInfoSource) : IUserCredentialNameByIdentityResolver
	where TUser : class
{
	private readonly Lazy<IUserCredentialNameByIdentityResolver> lazyInnerResolver = new(() =>
	{
		var identityInfo = identityInfoSource.GetIdentityInfo<TUser>();

		return (IUserCredentialNameByIdentityResolver)ActivatorUtilities.CreateInstance(
			serviceProvider,
			typeof(UserCredentialNameByIdentityResolver<,>).MakeGenericType(typeof(TUser), identityInfo.IdentityType),
			identityInfo);
	});

	public string? TryGetUserName(SecurityIdentity securityIdentity) => lazyInnerResolver.Value.TryGetUserName(securityIdentity);
}

public class UserCredentialNameByIdentityResolver<TUser, TIdent>(
	IQueryableSource queryableSource,
	UserSourceInfo<TUser> userSourceInfo,
	IdentityInfo<TUser, TIdent> identityInfo) : IUserCredentialNameByIdentityResolver
	where TUser : class
	where TIdent : notnull
{
	public string? TryGetUserName(SecurityIdentity securityIdentity)
	{
		var typedSecurityIdentity = securityIdentity as SecurityIdentity<TIdent> ?? throw new ArgumentOutOfRangeException(nameof(securityIdentity));

		return this.GetQueryable(typedSecurityIdentity).Select(userSourceInfo.NamePath).Select(v => (string?)v).SingleOrDefault();
	}

	private IQueryable<TUser> GetQueryable(SecurityIdentity<TIdent> securityIdentity)
	{
		return queryableSource
			.GetQueryable<TUser>()
			.Where(userSourceInfo.Filter)
			.Where(identityInfo.IdPath.Select(ExpressionHelper.GetEqualityWithExpr(securityIdentity.Id)));
	}
}