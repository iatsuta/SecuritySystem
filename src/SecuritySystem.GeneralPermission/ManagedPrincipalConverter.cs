using CommonFramework;
using CommonFramework.GenericRepository;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public class ManagedPrincipalConverter<TPrincipal>(
    IServiceProvider serviceProvider,
    IGeneralPermissionBindingInfoSource bindingInfoSource) : IManagedPrincipalConverter<TPrincipal>
{
    private readonly Lazy<IManagedPrincipalConverter<TPrincipal>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPrincipal(typeof(TPrincipal));

        //var permissionIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.PermissionType);

        var innerServiceType = typeof(ManagedPrincipalConverter<,,>).MakeGenericType(
            bindingInfo.PrincipalType,
            bindingInfo.PermissionType,
            bindingInfo.SecurityRoleType);

        return (IManagedPrincipalConverter<TPrincipal>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            bindingInfo);
    });

    public Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.ToManagedPrincipalAsync(principal, cancellationToken);
}

public class ManagedPrincipalConverter<TPrincipal, TPermission, TSecurityRole>(
    GeneralPermissionBindingInfo<TPermission, TPrincipal, TSecurityRole> bindingInfo,
    IQueryableSource queryableSource,
    IManagedPrincipalHeaderConverter<TPrincipal> headerConverter) : IManagedPrincipalConverter<TPrincipal>
    where TPrincipal : class
    where TPermission : class
{
    public async Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken)
    {
        var permissions = await queryableSource.GetQueryable<TPermission>()
            .Where(bindingInfo.Principal.Path.Select(p => p == principal))
            .GenericToListAsync(cancellationToken);

        return new ManagedPrincipal(
            headerConverter.Convert(principal),
            await permissions.SyncWhenAll(permission => this.ToManagedPermissionAsync(permission, cancellationToken)));
    }


    private async Task<ManagedPermission> ToManagedPermissionAsync(TPermission permission, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();

        //var restrictions =

        //new ManagedPermission(
        //    permission.Id,
        //    false,
        //    securityRoleSource.GetSecurityRole(permission.Role.Id),
        //    permission.Period.StartDate,
        //    permission.Period.EndDate,
        //    permission.Comment,
        //    permission.Restrictions
        //        .GroupBy(r => r.SecurityContextType.Id, r => r.SecurityContextId)
        //        .ToDictionary(
        //            g => securityContextInfoSource.GetSecurityContextInfo(g.Key).Type,
        //            Array (g) => g.ToArray()))

        //var getRestrictionsMethod = this.GetType().GetMethod(nameof(this.GetRestrictionArray), BindingFlags.Instance | BindingFlags.NonPublic)!;

        //var restrictions = bindingInfo
        //    .GetSecurityContextTypes()
        //    .Select(identityInfoSource.GetIdentityInfo)
        //    .Select(identityInfo =>
        //        (identityInfo.DomainObjectType, getRestrictionsMethod
        //            .MakeGenericMethod(identityInfo.DomainObjectType, identityInfo.IdentityType)
        //            .Invoke<Array>(this, permission, identityInfo)))
        //    .ToDictionary();

        //return new ManagedPermission(
        //    TypedSecurityIdentity.Create(permissionIdentityInfo.Id.Getter(permission)),
        //    true,
        //    bindingInfo.SecurityRole,
        //    bindingInfo.PeriodFilter == null ? (DateTime.MinValue, null) : this.expressionEvaluator.Evaluate(bindingInfo.PeriodFilter, permission),
        //    bindingInfo.CommentPath == null ? "Virtual Permission" : this.expressionEvaluator.Evaluate(bindingInfo.CommentPath, permission),
        //    restrictions);
    }
}