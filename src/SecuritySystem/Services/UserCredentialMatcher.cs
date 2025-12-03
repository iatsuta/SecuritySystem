using System.Numerics;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Credential;

namespace SecuritySystem.Services;

public class UserCredentialMatcher<TUser>(
	IServiceProvider serviceProvider,
	IIdentityInfoSource identityInfoSource,
	IVisualIdentityInfoSource visualIdentityInfoSource) : IUserCredentialMatcher<TUser>
{
	private readonly Lazy<IUserCredentialMatcher<TUser>> lazyInnerService = new(() =>
	{
		var identityInfo = identityInfoSource.GetIdentityInfo<TUser>();

		var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TUser>();

		var innerServiceType = typeof(UserCredentialMatcher<,>).MakeGenericType(typeof(TUser), identityInfo.IdentityType);

		return (IUserCredentialMatcher<TUser>)ActivatorUtilities.CreateInstance(serviceProvider, innerServiceType, innerServiceType, visualIdentityInfo);
	});

	public bool IsMatch(UserCredential userCredential, TUser user)
	{
		return this.lazyInnerService.Value.IsMatch(userCredential, user);
	}
}


public class UserCredentialMatcher<TUser, TIdent>(IdentityInfo<TUser, TIdent> identityInfo, VisualIdentityInfo<TUser> visualIdentityInfo)
	: IUserCredentialMatcher<TUser>
	where TIdent : IEqualityOperators<TIdent, TIdent, bool>, IParsable<TIdent>
{
	public bool IsMatch(UserCredential userCredential, TUser user)
	{
		switch (userCredential)
		{
			case UserCredential.IdentUserCredential { Identity : SecurityIdentity<TIdent> { Id: var id } }:
				return this.IsMatch(id, user);

			case UserCredential.NamedUserCredential { Name: var name }:
				return name.Equals(visualIdentityInfo.Name.Getter(user), StringComparison.CurrentCultureIgnoreCase);

			case UserCredential.UntypedIdentUserCredential { Id: var rawId } when TIdent.TryParse(rawId, null, out var id):
				return this.IsMatch(id, user);

			default:
				return false;
		}
	}

	private bool IsMatch(TIdent id, TUser user)
	{
		return identityInfo.Id.Getter(user) == id;
	}
}