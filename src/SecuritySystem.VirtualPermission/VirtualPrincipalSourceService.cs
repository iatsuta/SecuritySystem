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

using System.Linq.Expressions;
using System.Reflection;

namespace SecuritySystem.VirtualPermission;

public class VirtualPrincipalSourceService<TPrincipal, TPermission>(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource) : IPrincipalSourceService

	where TPrincipal : class
	where TPermission : class
{
	private readonly Lazy<IPrincipalSourceService> lazyPrincipalSourceService = new(() =>
		{
			var principalIdentityInfo = identityInfoSource.GetIdentityInfo<TPrincipal>();

			var permissionIdentityInfo = identityInfoSource.GetIdentityInfo<TPermission>();

			var innerServiceType = typeof(VirtualPrincipalSourceService<,,,>).MakeGenericType(typeof(TPrincipal), typeof(TPermission),
				principalIdentityInfo.IdentityType, permissionIdentityInfo.IdentityType);

			return (IPrincipalSourceService)ActivatorUtilities.CreateInstance(serviceProvider, innerServiceType, principalIdentityInfo, permissionIdentityInfo);
		});

	private IPrincipalSourceService PrincipalSourceService => this.lazyPrincipalSourceService.Value;

	public Task<IEnumerable<TypedPrincipalHeader>> GetPrincipalsAsync(string nameFilter, int limit, CancellationToken cancellationToken)
	{
		return this.PrincipalSourceService.GetPrincipalsAsync(nameFilter, limit, cancellationToken);
	}

	public Task<TypedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken)
	{
		return this.PrincipalSourceService.TryGetPrincipalAsync(userCredential, cancellationToken);
	}

	public Task<IEnumerable<string>> GetLinkedPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, CancellationToken cancellationToken)
	{
		return this.PrincipalSourceService.GetLinkedPrincipalsAsync(securityRoles, cancellationToken);
	}
}

public class VirtualPrincipalSourceService<TPrincipal, TPermission, TPrincipalIdent, TPermissionIdent>(
    IServiceProvider serviceProvider,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IQueryableSource queryableSource,
    IUserQueryableSource<TPrincipal> userQueryableSource,
    VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo,
    IIdentityInfoSource identityInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IdentityInfo<TPrincipal, TPrincipalIdent> principalIdentityInfo,
    IdentityInfo<TPermission, TPermissionIdent> permissionIdentityInfo) : IPrincipalSourceService

    where TPrincipal : class
    where TPermission : class
    where TPrincipalIdent : notnull
    where TPermissionIdent : notnull
{
    private readonly Expression<Func<TPrincipal, string>> principalNamePath = visualIdentityInfoSource.GetVisualIdentityInfo<TPrincipal>().Name.Path;

    private readonly IExpressionEvaluator expressionEvaluator =
        expressionEvaluatorStorage.GetForType(typeof(VirtualPrincipalSourceService<TPrincipal, TPermission, TPrincipalIdent, TPermissionIdent>));

    public async Task<IEnumerable<TypedPrincipalHeader>> GetPrincipalsAsync(
        string nameFilter,
        int limit,
        CancellationToken cancellationToken)
    {
        var toPrincipalAnonHeaderExpr =
            ExpressionEvaluateHelper.InlineEvaluate(ee =>
                ExpressionHelper.Create((TPrincipal principal) =>
                    new { Id = ee.Evaluate(principalIdentityInfo.Id.Path, principal), Name = ee.Evaluate(principalNamePath, principal) }));

        var anonHeaders = await queryableSource
            .GetQueryable<TPermission>()
            .Where(bindingInfo.GetFilter(serviceProvider))
            .Select(bindingInfo.PrincipalPath)
            .Where(
                string.IsNullOrWhiteSpace(nameFilter)
                    ? _ => true
                    : principalNamePath.Select(principalName => principalName.Contains(nameFilter)))
            .OrderBy(principalNamePath)
            .Take(limit)
            .Select(toPrincipalAnonHeaderExpr)
            .Distinct()
            .GenericToListAsync(cancellationToken);

        return anonHeaders.Select(anonHeader => new TypedPrincipalHeader(TypedSecurityIdentity.Create(anonHeader.Id), anonHeader.Name, true));
    }

    public async Task<TypedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken)
    {
        var principal = await userQueryableSource.GetQueryable(userCredential).GenericSingleOrDefaultAsync(cancellationToken);

        if (principal == null)
        {
            return null;
        }
        else
        {
            var header = new TypedPrincipalHeader(TypedSecurityIdentity.Create(principalIdentityInfo.Id.Getter(principal)),
                this.expressionEvaluator.Evaluate(principalNamePath, principal),
                true);

            var permissions = await queryableSource.GetQueryable<TPermission>()
                .Where(bindingInfo.GetFilter(serviceProvider))
                .Where(bindingInfo.PrincipalPath.Select(p => p == principal))
                .GenericToListAsync(cancellationToken);

            return new TypedPrincipal(header, permissions.Select(this.ToTypedPermission).ToList());
        }
    }

    private TypedPermission ToTypedPermission(TPermission permission)
    {
        var getRestrictionsMethod = this.GetType().GetMethod(nameof(this.GetRestrictionArray), BindingFlags.Instance | BindingFlags.NonPublic)!;

        var restrictions = bindingInfo
            .GetSecurityContextTypes()
            .Select(identityInfoSource.GetIdentityInfo)
            .Select(identityInfo =>
                (identityInfo.DomainObjectType, getRestrictionsMethod
                    .MakeGenericMethod(identityInfo.DomainObjectType, identityInfo.IdentityType)
                    .Invoke<Array>(this, permission, identityInfo)))
            .ToDictionary();

        return new TypedPermission(
            TypedSecurityIdentity.Create(permissionIdentityInfo.Id.Getter(permission)),
            true,
            bindingInfo.SecurityRole,
            bindingInfo.PeriodFilter == null ? (DateTime.MinValue, null) : this.expressionEvaluator.Evaluate(bindingInfo.PeriodFilter, permission),
            bindingInfo.CommentPath == null ? "Virtual Permission" : this.expressionEvaluator.Evaluate(bindingInfo.CommentPath, permission),
            restrictions);
    }

    public async Task<IEnumerable<string>> GetLinkedPrincipalsAsync(
        IEnumerable<SecurityRole> securityRoles,
        CancellationToken cancellationToken)
    {
        if (securityRoles.Contains(bindingInfo.SecurityRole))
        {
            return await queryableSource.GetQueryable<TPermission>()
                .Where(bindingInfo.GetFilter(serviceProvider))
                .Select(bindingInfo.PrincipalPath)
                .Select(principalNamePath)
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
        foreach (var restrictionPath in bindingInfo.Restrictions)
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