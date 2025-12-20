using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using SecuritySystem.ExternalSystem.Management;

using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.GeneralPermission;

public class ManagedPrincipalHeaderConverter<TPrincipal>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IGeneralPermissionBindingInfoSource bindingInfoSource) : IManagedPrincipalHeaderConverter<TPrincipal>
{
    private readonly Lazy<IManagedPrincipalHeaderConverter<TPrincipal>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPrincipal(typeof(TPrincipal));

        var principalIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.PrincipalType);

        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo(bindingInfo.PrincipalType);

        var innerServiceType = typeof(ManagedPrincipalHeaderConverter<,>).MakeGenericType(bindingInfo.PrincipalType, principalIdentityInfo.IdentityType);

        return (IManagedPrincipalHeaderConverter<TPrincipal>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            bindingInfo,
            principalIdentityInfo,
            visualIdentityInfo);
    });

    public Expression<Func<TPrincipal, ManagedPrincipalHeader>> ConvertExpression => this.lazyInnerService.Value.ConvertExpression;
    public ManagedPrincipalHeader Convert(TPrincipal principal) => this.lazyInnerService.Value.Convert(principal);
}

public class ManagedPrincipalHeaderConverter<TPrincipal, TPrincipalIdent>(
    GeneralPermissionBindingInfo bindingInfo,
    IdentityInfo<TPrincipal, TPrincipalIdent> principalIdentityInfo,
    VisualIdentityInfo<TPrincipal> visualIdentityInfo) : IManagedPrincipalHeaderConverter<TPrincipal>
    where TPrincipalIdent : notnull
{
    private Func<TPrincipal, ManagedPrincipalHeader>? convertFunc;

    public Expression<Func<TPrincipal, ManagedPrincipalHeader>> ConvertExpression { get; } =
        ExpressionEvaluateHelper.InlineEvaluate<Func<TPrincipal, ManagedPrincipalHeader>>(ee =>
            principal => new ManagedPrincipalHeader(
                new TypedSecurityIdentity<TPrincipalIdent>(ee.Evaluate(principalIdentityInfo.Id.Path, principal)),
                ee.Evaluate(visualIdentityInfo.Name.Path, principal),
                bindingInfo.IsReadonly));


    public ManagedPrincipalHeader Convert(TPrincipal principal) => (this.convertFunc ??= this.ConvertExpression.Compile()).Invoke(principal);
}