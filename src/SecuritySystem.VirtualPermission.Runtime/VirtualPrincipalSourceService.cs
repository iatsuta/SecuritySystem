using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.VirtualPermission;

public class VirtualPrincipalSourceService<TPermission>(
    IServiceProxyFactory serviceProxyFactory,
    IPermissionBindingInfoSource bindingInfoSource,
    VirtualPermissionBindingInfo<TPermission> virtualBindingInfo,
    IVisualIdentityInfoSource visualIdentityInfoSource) : IPrincipalSourceService
    where TPermission : class
{
    private readonly Lazy<IPrincipalSourceService> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo(bindingInfo.PrincipalType);

        var innerServiceType = typeof(VirtualPrincipalSourceService<,>).MakeGenericType(
            bindingInfo.PrincipalType,
            bindingInfo.PermissionType);

        return serviceProxyFactory.Create<IPrincipalSourceService>(innerServiceType, bindingInfo, virtualBindingInfo, principalVisualIdentityInfo);
    });

    private IPrincipalSourceService InnerService => this.lazyInnerService.Value;

    public Type PrincipalType => this.InnerService.PrincipalType;

    public IAsyncEnumerable<ManagedPrincipalHeader> GetPrincipalsAsync(string nameFilter, int limit) =>
        this.InnerService.GetPrincipalsAsync(nameFilter, limit);

    public Task<ManagedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken) =>
        this.InnerService.TryGetPrincipalAsync(userCredential, cancellationToken);

    public IAsyncEnumerable<string> GetLinkedPrincipalsAsync(ImmutableHashSet<SecurityRole> securityRoles) => this.InnerService.GetLinkedPrincipalsAsync(securityRoles);
}

public class VirtualPrincipalSourceService<TPrincipal, TPermission>(
    IServiceProvider serviceProvider,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IQueryableSource queryableSource,
    IUserQueryableSource<TPrincipal> userQueryableSource,
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    VirtualPermissionBindingInfo<TPermission> virtualBindingInfo,
    IIdentityInfoSource identityInfoSource,
    IManagedPrincipalHeaderConverterFactory<TPrincipal> managedPrincipalHeaderConverterFactory,
    ISecurityIdentityExtractor<TPermission> permissionIdentityExtractor,
    VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo) : IPrincipalSourceService

    where TPrincipal : class
    where TPermission : class
{
    private readonly IManagedPrincipalHeaderConverter<TPrincipal> managedPrincipalHeaderConverter =
        managedPrincipalHeaderConverterFactory.Create(bindingInfo);

    private readonly IExpressionEvaluator expressionEvaluator =
        expressionEvaluatorStorage.GetForType(typeof(VirtualPrincipalSourceService<TPrincipal, TPermission>));

    public Type PrincipalType { get; } = typeof(TPrincipal);

    public IAsyncEnumerable<ManagedPrincipalHeader> GetPrincipalsAsync(string nameFilter, int limit) =>

        virtualBindingInfo
            .Items
            .ToAsyncEnumerable()
            .SelectMany(itemBindingInfo =>
            {
                return queryableSource
                    .GetQueryable<TPermission>()
                    .Where(itemBindingInfo.Filter(serviceProvider))
                    .Select(bindingInfo.Principal.Path)
                    .Where(
                        string.IsNullOrWhiteSpace(nameFilter)
                            ? _ => true
                            : principalVisualIdentityInfo.Name.Path.Select(principalName => principalName.Contains(nameFilter)))
                    .OrderBy(principalVisualIdentityInfo.Name.Path)
                    .Take(limit)
                    .Select(managedPrincipalHeaderConverter.ConvertExpression)
                    .Distinct()
                    .GenericAsAsyncEnumerable();
            }).Take(limit).Distinct();

    public async Task<ManagedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken)
    {
        var principal = await userQueryableSource.GetQueryable(userCredential).GenericSingleOrDefaultAsync(cancellationToken);

        if (principal == null)
        {
            return null;
        }
        else
        {
            var header = managedPrincipalHeaderConverter.Convert(principal);

            var managedPermissions = await virtualBindingInfo
                .Items
                .ToAsyncEnumerable()
                .SelectMany(async (itemBindingInfo, ct) =>
                {
                    var permissions = await queryableSource.GetQueryable<TPermission>()
                        .Where(itemBindingInfo.Filter(serviceProvider))
                        .Where(bindingInfo.Principal.Path.Select(p => p == principal))
                        .GenericToListAsync(ct);

                    return permissions.Select(permission => this.ToManagedPermission(permission, itemBindingInfo.SecurityRole));
                })
                .ToArrayAsync(cancellationToken);

            return new ManagedPrincipal(header, [..managedPermissions]);
        }
    }

    private ManagedPermission ToManagedPermission(TPermission permission, SecurityRole securityRole)
    {
        var getRestrictionsMethod = this.GetType().GetMethod(nameof(this.GetRestrictionArray), BindingFlags.Instance | BindingFlags.NonPublic)!;

        var restrictions = virtualBindingInfo
            .SecurityContextTypes
            .Select(identityInfoSource.GetIdentityInfo)
            .Select(identityInfo =>
                (identityInfo.DomainObjectType, getRestrictionsMethod
                    .MakeGenericMethod(identityInfo.DomainObjectType, identityInfo.IdentityType)
                    .Invoke<Array>(this, permission, identityInfo)))
            .ToImmutableDictionary();

        return new ManagedPermission
        {
            Identity = permissionIdentityExtractor.Extract(permission),
            IsVirtual = true,
            SecurityRole = securityRole,
            Period = bindingInfo.GetSafePeriod(permission),
            Comment = bindingInfo.GetSafeComment(permission),
            DelegatedFrom = bindingInfo.DelegatedFrom?.Getter.Invoke(permission) is { } delegatedFromPermission
                ? permissionIdentityExtractor.Extract(delegatedFromPermission)
                : SecurityIdentity.Default,
            Restrictions = restrictions
        };
    }

    public IAsyncEnumerable<string> GetLinkedPrincipalsAsync(ImmutableHashSet<SecurityRole> securityRoles)
    {
        return virtualBindingInfo
            .Items
            .ToAsyncEnumerable()
            .Where(itemBindingInfo => securityRoles.Contains(itemBindingInfo.SecurityRole))
            .SelectMany(itemBindingInfo => queryableSource.GetQueryable<TPermission>()
                .Where(itemBindingInfo.Filter(serviceProvider))
                .Select(bindingInfo.Principal.Path)
                .Select(principalVisualIdentityInfo.Name.Path)
                .GenericAsAsyncEnumerable())
            .Distinct();
    }

    private TSecurityContextIdent[] GetRestrictionArray<TSecurityContext, TSecurityContextIdent>(
        TPermission permission,
        IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
        where TSecurityContext : ISecurityContext
        where TSecurityContextIdent : notnull
    {
        return this.GetRestrictionIdents(permission, identityInfo).ToArray();
    }

    private IEnumerable<TSecurityContextIdent> GetRestrictionIdents<TSecurityContext, TSecurityContextIdent>(
        TPermission permission,
        IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
        where TSecurityContext : ISecurityContext
        where TSecurityContextIdent : notnull
    {
        foreach (var restrictionPath in virtualBindingInfo.Restrictions)
        {
            if (restrictionPath is Expression<Func<TPermission, TSecurityContext?>> singlePath)
            {
                var securityContext = this.expressionEvaluator.Evaluate(singlePath, permission);

                if (securityContext != null)
                {
                    yield return identityInfo.Id.Getter(securityContext);
                }
            }
            else if (restrictionPath is Expression<Func<TPermission, IEnumerable<TSecurityContext>>> manyPath)
            {
                foreach (var securityContext in this.expressionEvaluator.Evaluate(manyPath, permission))
                {
                    yield return identityInfo.Id.Getter(securityContext);
                }
            }
        }
    }
}