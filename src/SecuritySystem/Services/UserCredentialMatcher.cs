using CommonFramework.DependencyInjection;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using SecuritySystem.Credential;

namespace SecuritySystem.Services;

public class UserCredentialMatcher<TUser>(
    IServiceProxyFactory serviceProxyFactory,
    IIdentityInfoSource identityInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource) : IUserCredentialMatcher<TUser>
{
    private readonly Lazy<IUserCredentialMatcher<TUser>> lazyInnerService = new(() =>
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TUser>();

        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TUser>();

        var innerServiceType = typeof(UserCredentialMatcher<,>).MakeGenericType(typeof(TUser), identityInfo.IdentityType);

        return serviceProxyFactory.Create<IUserCredentialMatcher<TUser>>(innerServiceType, identityInfo, visualIdentityInfo);
    });

    public bool IsMatch(UserCredential userCredential, TUser user) => this.lazyInnerService.Value.IsMatch(userCredential, user);
}

public class UserCredentialMatcher<TUser, TIdent>(
    IdentityInfo<TUser, TIdent> identityInfo,
    VisualIdentityInfo<TUser> visualIdentityInfo,
    ISecurityIdentityConverter<TIdent> securityIdentityConverter)
    : IUserCredentialMatcher<TUser>
    where TIdent : notnull
{
    public bool IsMatch(UserCredential userCredential, TUser user)
    {
        switch (userCredential)
        {
            case UserCredential.IdentUserCredential { Identity : var identity } when securityIdentityConverter.TryConvert(identity) is { } typedIdentity:
            {
                return EqualityComparer<TIdent>.Default.Equals(identityInfo.Id.Getter(user), typedIdentity.Id);
            }

            case UserCredential.NamedUserCredential { Name: var name }:
                return name.Equals(visualIdentityInfo.Name.Getter(user), StringComparison.CurrentCultureIgnoreCase);

            default:
                return false;
        }
    }
}