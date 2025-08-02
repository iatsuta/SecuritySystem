using CommonFramework;

using SecuritySystem.Credential;
using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

using System.Linq.Expressions;
using System.Reflection;
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

    public async Task<IEnumerable<TypedPrincipalHeader>> GetPrincipalsAsync(
        string nameFilter,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var principalNamePath = bindingInfo.PrincipalNamePath;

        var toPrincipalAnonHeaderExpr =
            ExpressionEvaluateHelper.InlineEvaluate(ee =>
                ExpressionHelper.Create((TPrincipal principal) => new { principal.Id, Name = ee.Evaluate(principalNamePath, principal) }));

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
                principal => principal.Id == principalId,
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
            var header = new TypedPrincipalHeader(principal.Id, this.expressionEvaluator.Evaluate(bindingInfo.PrincipalNamePath, principal), true);

            var permissions = await queryableSource.GetQueryable<TPermission>()
                                                   .Where(bindingInfo.GetFilter(serviceProvider))
                                                   .Where(bindingInfo.PrincipalPath.Select(filter))
                                                   .GenericToListAsync(cancellationToken);

            return new TypedPrincipal(header, permissions.Select(this.ToTypedPermission).ToList());
        }
    }

    private TypedPermission ToTypedPermission(TPermission permission)
    {
        var getRestrictionsMethod = this.GetType().GetMethod(nameof(this.GetRestrictions), BindingFlags.Instance | BindingFlags.NonPublic)!;

        var restrictions = bindingInfo
                           .GetSecurityContextTypes()
                           .Select(
                               securityContextType => (securityContextType, getRestrictionsMethod
                                                                            .MakeGenericMethod(securityContextType)
                                                                            .Invoke<IEnumerable<Guid>>(this, permission)
                                                                            .ToReadOnlyListI()))
                           .ToDictionary();

        return new TypedPermission(
            permission.Id,
            true,
            bindingInfo.SecurityRole,
            bindingInfo.PeriodFilter != null ? this.expressionEvaluator.Evaluate(bindingInfo.PeriodFilter, permission) : Period.Eternity,
            "",
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

    private IEnumerable<Guid> GetRestrictions<TSecurityContext>(TPermission permission)
        where TSecurityContext : ISecurityContext
    {
        foreach (var restrictionPath in bindingInfo.RestrictionPaths)
        {
            if (restrictionPath is Expression<Func<TPermission, TSecurityContext?>> singlePath)
            {
                var securityContext = this.expressionEvaluator.Evaluate(singlePath, permission);

                if (securityContext != null)
                {
                    yield return securityContext.Id;
                }
            }
            else if (restrictionPath is Expression<Func<TPermission, IEnumerable<TSecurityContext>>> manyPath)
            {
                foreach (var securityContext in this.expressionEvaluator.Evaluate(manyPath, permission))
                {
                    yield return securityContext.Id;
                }
            }
        }
    }
}
