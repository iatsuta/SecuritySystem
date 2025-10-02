using CommonFramework;

using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

using System.Linq.Expressions;
using System.Reflection;

using CommonFramework.ExpressionEvaluate;

using GenericQueryable;

namespace SecuritySystem.VirtualPermission;

public class VirtualPrincipalSourceService<TPrincipal, TPermission>(
    IServiceProvider serviceProvider,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    IQueryableSource queryableSource,
    IIdentityInfoSource identityInfoSource,
    VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo) : IPrincipalSourceService

    where TPrincipal : class
    where TPermission : class
{
    private readonly IExpressionEvaluator expressionEvaluator = expressionEvaluatorStorage.GetForType(typeof(VirtualPrincipalSourceService<TPrincipal, TPermission>));

    private readonly IdentityInfo<TPrincipal, Guid> principalIdentityInfo = identityInfoSource.GetIdentityInfo<TPrincipal, Guid>();

    private readonly IdentityInfo<TPermission, Guid> permissionIdentityInfo = identityInfoSource.GetIdentityInfo<TPermission, Guid>();

    public async Task<IEnumerable<TypedPrincipalHeader>> GetPrincipalsAsync(
        string nameFilter,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var principalNamePath = bindingInfo.PrincipalNamePath;

        var toPrincipalAnonHeaderExpr =
            ExpressionEvaluateHelper.InlineEvaluate(ee =>
                ExpressionHelper.Create((TPrincipal principal) =>
                    new { Id = ee.Evaluate(principalIdentityInfo.IdPath, principal), Name = ee.Evaluate(principalNamePath, principal) }));

        var anonHeaders = await queryableSource
            .GetQueryable<TPermission>()
            .Where(bindingInfo.GetFilter(serviceProvider))
            .Select(bindingInfo.PrincipalPath)
            .Where(
                string.IsNullOrWhiteSpace(nameFilter)
                    ? _ => true
                    : bindingInfo.PrincipalNamePath.Select(principalName => principalName.Contains(nameFilter)))
            .OrderBy(bindingInfo.PrincipalNamePath)
            .Take(limit)
            .Select(toPrincipalAnonHeaderExpr)
            .Distinct()
            .GenericToListAsync(cancellationToken);

        return anonHeaders.Select(anonHeader => new TypedPrincipalHeader(anonHeader.Id, anonHeader.Name, true));
    }

    public Task<TypedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken = default) =>
        userCredential switch
        {
            UserCredential.NamedUserCredential { Name: var principalName } => this.TryGetPrincipalAsync(
                bindingInfo.PrincipalNamePath.Select(name => principalName == name),
                cancellationToken),

            UserCredential.IdentUserCredential { Id: var principalId } => this.TryGetPrincipalAsync(
                principalIdentityInfo.IdPath.Select(id => principalId == id),
                cancellationToken),

            _ => throw new ArgumentOutOfRangeException(nameof(userCredential))
        };

    public async Task<TypedPrincipal?> TryGetPrincipalAsync(Expression<Func<TPrincipal, bool>> filter, CancellationToken cancellationToken)
    {
        var principal = await queryableSource.GetQueryable<TPrincipal>()
            .Where(filter)
            .GenericSingleOrDefaultAsync(cancellationToken);

        if (principal == null)
        {
            return null;
        }
        else
        {
            var header = new TypedPrincipalHeader(principalIdentityInfo.IdFunc(principal), this.expressionEvaluator.Evaluate(bindingInfo.PrincipalNamePath, principal),
                true);

            var permissions = await queryableSource.GetQueryable<TPermission>()
                .Where(bindingInfo.GetFilter(serviceProvider))
                .Where(bindingInfo.PrincipalPath.Select(filter))
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
            permissionIdentityInfo.IdFunc(permission),
            true,
            bindingInfo.SecurityRole,
            bindingInfo.StartDateFilter == null ? DateTime.MinValue : this.expressionEvaluator.Evaluate(bindingInfo.StartDateFilter, permission),
            bindingInfo.EndDateFilter == null ? null : this.expressionEvaluator.Evaluate(bindingInfo.EndDateFilter, permission),
            "Virtual Permission",
            restrictions);
    }

    public async Task<IEnumerable<string>> GetLinkedPrincipalsAsync(
        IEnumerable<SecurityRole> securityRoles,
        CancellationToken cancellationToken = default)
    {
        if (securityRoles.Contains(bindingInfo.SecurityRole))
        {
            return await queryableSource.GetQueryable<TPermission>()
                .Where(bindingInfo.GetFilter(serviceProvider))
                .Select(bindingInfo.PrincipalPath)
                .Select(bindingInfo.PrincipalNamePath)
                .GenericToListAsync(cancellationToken);
        }
        else
        {
            return [];
        }
    }
    private Array GetRestrictionArray<TSecurityContext, TIdent>(TPermission permission, IdentityInfo<TSecurityContext, TIdent> identityInfo)
        where TSecurityContext : ISecurityContext
        where TIdent : notnull
    {
        return this.GetRestrictionIdents(permission, identityInfo).ToArray();
    }

    private IEnumerable<TIdent> GetRestrictionIdents<TSecurityContext, TIdent>(TPermission permission, IdentityInfo<TSecurityContext, TIdent> identityInfo)
        where TSecurityContext : ISecurityContext
        where TIdent : notnull
    {
        foreach (var restrictionPath in bindingInfo.RestrictionPaths)
        {
            if (restrictionPath is Expression<Func<TPermission, TSecurityContext?>> singlePath)
            {
                var securityContext = this.expressionEvaluator.Evaluate(singlePath, permission);

                if (securityContext != null)
                {
                    yield return identityInfo.IdFunc(securityContext);
                }
            }
            else if (restrictionPath is Expression<Func<TPermission, IEnumerable<TSecurityContext>>> manyPath)
            {
                foreach (var securityContext in this.expressionEvaluator.Evaluate(manyPath, permission))
                {
                    yield return identityInfo.IdFunc(securityContext);
                }
            }
        }
    }
}