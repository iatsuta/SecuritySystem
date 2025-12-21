using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.UserSource;
using SecuritySystem.Credential;
using SecuritySystem.Services;

using System.Linq.Expressions;
using System.Reflection;

namespace SecuritySystem.VirtualPermission;

public class VirtualPrincipalSourceService(
    IServiceProvider serviceProvider,
    VirtualPermissionBindingInfo virtualBindingInfo,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IIdentityInfoSource identityInfoSource) : IPrincipalSourceService
{
    public Task<IEnumerable<ManagedPrincipalHeader>> GetPrincipalsAsync(string nameFilter, int limit, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ManagedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetLinkedPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Type PrincipalType { get; }
}

public class VirtualPrincipalSourceService<TPrincipal, TPermission>(
    IServiceProvider serviceProvider,
    VirtualPermissionBindingInfo<TPermission> virtualBindingInfo,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IIdentityInfoSource identityInfoSource) : IPrincipalSourceService

	where TPrincipal : class
	where TPermission : class
{
    private readonly Lazy<IPrincipalSourceService> lazyInnerService = new(() =>
    {
        var principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TPrincipal>();

        var permissionIdentityInfo = identityInfoSource.GetIdentityInfo<TPermission>();

        var innerServiceType = typeof(VirtualPrincipalSourceService<,,>).MakeGenericType(
            typeof(TPrincipal),
            typeof(TPermission),
            permissionIdentityInfo.IdentityType);

        return (IPrincipalSourceService)ActivatorUtilities.CreateInstance(serviceProvider, innerServiceType, virtualBindingInfo, principalVisualIdentityInfo, permissionIdentityInfo);
    });

    private IPrincipalSourceService InnerService => this.lazyInnerService.Value;

    public Type PrincipalType => this.InnerService.PrincipalType;

    public Task<IEnumerable<ManagedPrincipalHeader>> GetPrincipalsAsync(string nameFilter, int limit, CancellationToken cancellationToken)
	{
		return this.InnerService.GetPrincipalsAsync(nameFilter, limit, cancellationToken);
	}

	public Task<ManagedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken)
	{
		return this.InnerService.TryGetPrincipalAsync(userCredential, cancellationToken);
	}

	public Task<IEnumerable<string>> GetLinkedPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, CancellationToken cancellationToken)
	{
		return this.InnerService.GetLinkedPrincipalsAsync(securityRoles, cancellationToken);
	}
}

public class VirtualPrincipalSourceService<TPrincipal, TPermission, TPermissionIdent>(
    IServiceProvider serviceProvider,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IQueryableSource queryableSource,
    IUserQueryableSource<TPrincipal> userQueryableSource,
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    VirtualPermissionBindingInfo<TPermission> virtualBindingInfo,
    IIdentityInfoSource identityInfoSource,
    IManagedPrincipalHeaderConverter<TPrincipal> managedPrincipalHeaderConverter,
    VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo,
    IdentityInfo<TPermission, TPermissionIdent> permissionIdentityInfo) : IPrincipalSourceService

    where TPrincipal : class
    where TPermission : class
    where TPermissionIdent : notnull
{
    private readonly IExpressionEvaluator expressionEvaluator =
        expressionEvaluatorStorage.GetForType(typeof(VirtualPrincipalSourceService<TPrincipal, TPermission, TPermissionIdent>));

    public Type PrincipalType { get; } = typeof(TPrincipal);

    public async Task<IEnumerable<ManagedPrincipalHeader>> GetPrincipalsAsync(
        string nameFilter,
        int limit,
        CancellationToken cancellationToken)
    {
        return await queryableSource
            .GetQueryable<TPermission>()
            .Where(virtualBindingInfo.GetFilter(serviceProvider))
            .Select(bindingInfo.Principal.Path)
            .Where(
                string.IsNullOrWhiteSpace(nameFilter)
                    ? _ => true
                    : principalVisualIdentityInfo.Name.Path.Select(principalName => principalName.Contains(nameFilter)))
            .OrderBy(principalVisualIdentityInfo.Name.Path)
            .Take(limit)
            .Distinct()
            .Select(managedPrincipalHeaderConverter.ConvertExpression)
            .GenericToListAsync(cancellationToken);
    }

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

            var permissions = await queryableSource.GetQueryable<TPermission>()
                .Where(virtualBindingInfo.GetFilter(serviceProvider))
                .Where(bindingInfo.Principal.Path.Select(p => p == principal))
                .GenericToListAsync(cancellationToken);

            return new ManagedPrincipal(header, permissions.Select(this.ToManagedPermission).ToList());
        }
    }

    private ManagedPermission ToManagedPermission(TPermission permission)
    {
        var getRestrictionsMethod = this.GetType().GetMethod(nameof(this.GetRestrictionArray), BindingFlags.Instance | BindingFlags.NonPublic)!;

        var restrictions = virtualBindingInfo
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
            virtualBindingInfo.SecurityRole,
            bindingInfo.GetSafePeriod(permission),
            bindingInfo.GetSafeComment(permission),
            restrictions);
    }

    public async Task<IEnumerable<string>> GetLinkedPrincipalsAsync(
        IEnumerable<SecurityRole> securityRoles,
        CancellationToken cancellationToken)
    {
        if (securityRoles.Contains(virtualBindingInfo.SecurityRole))
        {
            return await queryableSource.GetQueryable<TPermission>()
                .Where(virtualBindingInfo.GetFilter(serviceProvider))
                .Select(bindingInfo.Principal.Path)
                .Select(principalVisualIdentityInfo.Name.Path)
                .GenericToListAsync(cancellationToken);
        }
        else
        {
            return [];
        }
    }

    private Array GetRestrictionArray<TSecurityContext, TSecurityContextIdent>(TPermission permission,
        IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
        where TSecurityContext : ISecurityContext
        where TSecurityContextIdent : notnull
    {
        return this.GetRestrictionIdents(permission, identityInfo).ToArray();
    }

    private IEnumerable<TSecurityContextIdent> GetRestrictionIdents<TSecurityContext, TSecurityContextIdent>(TPermission permission,
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