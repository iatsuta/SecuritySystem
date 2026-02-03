using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using SecuritySystem.ExternalSystem.Management;

using System.Linq.Expressions;

namespace SecuritySystem.Services;

public class ManagedPrincipalHeaderConverter<TPrincipal>(
    IServiceProxyFactory serviceProxyFactory,
    IIdentityInfoSource identityInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IPermissionBindingInfoSource bindingInfoSource,
    Tuple<PermissionBindingInfo>? customBindingInfo = null) : IManagedPrincipalHeaderConverter<TPrincipal>
{
    private readonly Lazy<IManagedPrincipalHeaderConverter<TPrincipal>> lazyInnerService = new(() =>
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TPrincipal>();

        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TPrincipal>();

        var innerServiceType = typeof(ManagedPrincipalHeaderConverter<,>).MakeGenericType(identityInfo.DomainObjectType, identityInfo.IdentityType);

        return serviceProxyFactory.Create<IManagedPrincipalHeaderConverter<TPrincipal>>(
            innerServiceType,
            customBindingInfo?.Item1 ?? bindingInfoSource.GetForPrincipal(typeof(TPrincipal)),
            identityInfo,
            visualIdentityInfo);
    });

    public Expression<Func<TPrincipal, ManagedPrincipalHeader>> ConvertExpression => this.lazyInnerService.Value.ConvertExpression;

    public ManagedPrincipalHeader Convert(TPrincipal principal) => this.lazyInnerService.Value.Convert(principal);
}

public class ManagedPrincipalHeaderConverter<TPrincipal, TPrincipalIdent>(
    PermissionBindingInfo bindingInfo,
    IdentityInfo<TPrincipal, TPrincipalIdent> identityInfo,
    VisualIdentityInfo<TPrincipal> visualIdentityInfo) : IManagedPrincipalHeaderConverter<TPrincipal>
    where TPrincipalIdent : notnull
{
    private Func<TPrincipal, ManagedPrincipalHeader>? convertFunc;

    public Expression<Func<TPrincipal, ManagedPrincipalHeader>> ConvertExpression { get; } =
        ExpressionEvaluateHelper.InlineEvaluate<Func<TPrincipal, ManagedPrincipalHeader>>(ee =>
            principal => new ManagedPrincipalHeader(
                new TypedSecurityIdentity<TPrincipalIdent>(ee.Evaluate(identityInfo.Id.Path, principal)),
                ee.Evaluate(visualIdentityInfo.Name.Path, principal),
                bindingInfo.IsReadonly));

    public ManagedPrincipalHeader Convert(TPrincipal principal) => (this.convertFunc ??= this.ConvertExpression.Compile()).Invoke(principal);
}