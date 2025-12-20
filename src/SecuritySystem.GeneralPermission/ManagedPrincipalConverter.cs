using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;
using GenericQueryable;
using Microsoft.Extensions.DependencyInjection;
using SecuritySystem.ExternalSystem.Management;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;

namespace SecuritySystem.GeneralPermission;

public class ManagedPrincipalConverter<TPrincipal>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IGeneralPermissionBindingInfoSource bindingInfoSource) : IManagedPrincipalConverter<TPrincipal>
{
    private readonly Lazy<IManagedPrincipalConverter<TPrincipal>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPrincipal));

        var identityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.PrincipalType);

        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo(bindingInfo.PrincipalType);

        var innerServiceType = typeof(ManagedPrincipalConverter<,>).MakeGenericType(bindingInfo.PrincipalType, identityInfo.IdentityType);

        return (IManagedPrincipalConverter<TPrincipal>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            bindingInfo,
            identityInfo,
            visualIdentityInfo);
    });

    public Expression<Func<TPrincipal, ManagedPrincipalHeader>> GetToHeaderExpression() => this.lazyInnerService.Value.GetToHeaderExpression();

    public Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.ToManagedPrincipalAsync(principal, cancellationToken);
}

public class ManagedPrincipalConverter<TPrincipal, TPermission, TSecurityRole, TPrincipalIdent>(
    GeneralPermissionBindingInfo<TPermission, TPrincipal, TSecurityRole> bindingInfo,
    IQueryableSource queryableSource,
    IdentityInfo<TPrincipal, TPrincipalIdent> identityInfo,
    VisualIdentityInfo<TPrincipal> visualIdentityInfo) : IManagedPrincipalConverter<TPrincipal>
	where TPrincipalIdent : notnull where TPermission : class
{
    private readonly Expression<Func<TPrincipal, ManagedPrincipalHeader>> convertToHeaderExpression =
        ExpressionEvaluateHelper.InlineEvaluate<Func<TPrincipal, ManagedPrincipalHeader>>(ee =>
            principal => new ManagedPrincipalHeader(
                new TypedSecurityIdentity<TPrincipalIdent>(ee.Evaluate(identityInfo.Id.Path, principal)),
                ee.Evaluate(visualIdentityInfo.Name.Path, principal),
                bindingInfo.IsReadonly));

    private Func<TPrincipal, ManagedPrincipalHeader>? convertToHeaderFunc;

    public Expression<Func<TPrincipal, ManagedPrincipalHeader>> GetToHeaderExpression() => this.convertToHeaderExpression;

    public async Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken)
    {
        var convertFunc = convertToHeaderFunc ??= convertToHeaderExpression.Compile();

        var permissions = await queryableSource.GetQueryable<TPermission>()
            .Where(bindingInfo.Principal.Path.Select(ExpressionHelper.GetEqualityWithExpr(principal)))
            .GenericToListAsync(cancellationToken);

        return new ManagedPrincipal(
            convertFunc(principal),
            await permissions.SyncWhenAll(permission => this.ToManagedPermissionAsync(permission, cancellationToken)));
    }


    private async Task<ManagedPermission> ToManagedPermissionAsync(TPermission permission, CancellationToken cancellationToken)
    {
        var restrictions =

        new ManagedPermission(
            permission.Id,
            false,
            securityRoleSource.GetSecurityRole(permission.Role.Id),
            permission.Period.StartDate,
            permission.Period.EndDate,
            permission.Comment,
            permission.Restrictions
                .GroupBy(r => r.SecurityContextType.Id, r => r.SecurityContextId)
                .ToDictionary(
                    g => securityContextInfoSource.GetSecurityContextInfo(g.Key).Type,
                    Array (g) => g.ToArray()))

        var getRestrictionsMethod = this.GetType().GetMethod(nameof(this.GetRestrictionArray), BindingFlags.Instance | BindingFlags.NonPublic)!;

        var restrictions = bindingInfo
            .GetSecurityContextTypes()
            .Select(identityInfoSource.GetIdentityInfo)
            .Select(identityInfo =>
                (identityInfo.DomainObjectType, getRestrictionsMethod
                    .MakeGenericMethod(identityInfo.DomainObjectType, identityInfo.IdentityType)
                    .Invoke<Array>(this, permission, identityInfo)))
            .ToDictionary();

        return new ManagedPermission(
            TypedSecurityIdentity.Create(permissionIdentityInfo.Id.Getter(permission)),
            true,
            bindingInfo.SecurityRole,
            bindingInfo.PeriodFilter == null ? (DateTime.MinValue, null) : this.expressionEvaluator.Evaluate(bindingInfo.PeriodFilter, permission),
            bindingInfo.CommentPath == null ? "Virtual Permission" : this.expressionEvaluator.Evaluate(bindingInfo.CommentPath, permission),
            restrictions);
    }
}