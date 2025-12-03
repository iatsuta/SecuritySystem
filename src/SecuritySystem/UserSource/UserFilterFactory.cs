using CommonFramework;

using SecuritySystem.Credential;
using SecuritySystem.Services;

using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.UserSource;

public class UserFilterFactory<TUser>(
	IServiceProvider serviceProvider,
	IIdentityInfoSource identityInfoSource,
	IVisualIdentityInfoSource visualIdentityInfoSource) : IUserFilterFactory<TUser>
{
	private readonly Lazy<IUserFilterFactory<TUser>> lazyInnerService = new(() =>
	{
		var identityInfo = identityInfoSource.GetIdentityInfo(typeof(TUser));

		var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TUser>();

		return (IUserFilterFactory<TUser>)
			ActivatorUtilities.CreateInstance(
				serviceProvider,
				typeof(UserFilterFactory<,>).MakeGenericType(typeof(TUser), identityInfo.IdentityType), identityInfo, visualIdentityInfo);
	});

	public Expression<Func<TUser, bool>> CreateFilter(UserCredential userCredential)
	{
		return this.lazyInnerService.Value.CreateFilter(userCredential);
	}
}

public class UserFilterFactory<TUser, TIdent>(
	IdentityInfo<TUser, TIdent> identityInfo,
	VisualIdentityInfo<TUser> visualIdentityInfo,
	ISecurityIdentityConverter<TIdent> identityConverter) : IUserFilterFactory<TUser>
	where TUser : class
	where TIdent : IParsable<TIdent>
{
	public Expression<Func<TUser, bool>> CreateFilter(UserCredential userCredential)
	{
		switch (userCredential)
		{
			case UserCredential.NamedUserCredential { Name: var name }:
				return visualIdentityInfo.Name.Path.Select(objName => objName == name);

			case UserCredential.IdentUserCredential { Identity: var identity }:
			{
				var convertedIdentity = identityConverter.TryConvert(identity);

				if (convertedIdentity == null)
				{
					return _ => false;
				}
				else
				{
					return identityInfo.Id.Path.Select(ExpressionHelper.GetEqualityWithExpr(convertedIdentity.Id));
				}
			}

			case UserCredential.UntypedIdentUserCredential { Id: var rawId } when TIdent.TryParse(rawId, null, out var id):
				return identityInfo.Id.Path.Select(ExpressionHelper.GetEqualityWithExpr(id));

			default:
				throw new ArgumentOutOfRangeException(nameof(userCredential));
		}
	}
}