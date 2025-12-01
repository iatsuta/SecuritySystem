using CommonFramework;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.GeneralPermission.Validation;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.GeneralPermission;

public class PrincipalDomainService<TPrincipal>(
    IServiceProvider serviceProvider,
    GeneralPermissionSystemInfo<TPrincipal> info,
    IIdentityInfoSource identityInfoSource) : IPrincipalDomainService<TPrincipal>
{
    private readonly Lazy<IPrincipalDomainService<TPrincipal>> lazyInnerService = new(() =>
    {
        var identityInfo = identityInfoSource.GetIdentityInfo(typeof(TPrincipal));

        var innerServiceType = typeof(PrincipalDomainService<,,>).MakeGenericType(typeof(TPrincipal), info.PermissionType, identityInfo.IdentityType);

        return (IPrincipalDomainService<TPrincipal>)ActivatorUtilities.CreateInstance(serviceProvider, innerServiceType, identityInfo);
    });

    private IPrincipalDomainService<TPrincipal> InnerService => this.lazyInnerService.Value;

    public Task<TPrincipal> GetOrCreateAsync(string name, CancellationToken cancellationToken) =>
        this.InnerService.GetOrCreateAsync(name, cancellationToken);

    public Task SaveAsync(TPrincipal principal, CancellationToken cancellationToken) =>
        this.InnerService.SaveAsync(principal, cancellationToken);

    public Task RemoveAsync(TPrincipal principal, bool force, CancellationToken cancellationToken) =>
        this.InnerService.RemoveAsync(principal, force, cancellationToken);

    public Task ValidateAsync(TPrincipal principal, CancellationToken cancellationToken) =>
        this.InnerService.ValidateAsync(principal, cancellationToken);
}

public class PrincipalDomainService<TPrincipal, TPermission, TPrincipalIdent>(
    IQueryableSource queryableSource,
    IGenericRepository genericRepository,
    IPrincipalGeneralValidator<TPrincipal> principalGeneralValidator,
    UserSourceInfo<TPrincipal> userSourceInfo,
    IEnumerable<IUserSource> userSources,
    IPermissionToPrincipalInfo<TPrincipal, TPermission> permissionToPrincipalInfo,
    ISecurityIdentityConverter<TPrincipalIdent> identityConverter,
    IdentityInfo<TPrincipal, TPrincipalIdent> identityInfo) : IPrincipalDomainService<TPrincipal>
    where TPrincipal : class, new()
    where TPrincipalIdent : notnull
    where TPermission : class
{
    public async Task<TPrincipal> GetOrCreateAsync(string name, CancellationToken cancellationToken)
    {
        var principal = await queryableSource.GetQueryable<TPrincipal>()
            .GenericSingleOrDefaultAsync(userSourceInfo.Name.Path.Select(ExpressionHelper.GetEqualityWithExpr(name)), cancellationToken);

        if (principal is null)
        {
            principal = new TPrincipal();

            userSourceInfo.Name.Setter(principal, name);

            var ident = await this.TryExtractIdent(name, cancellationToken);

            if (ident is not null)
            {
                identityInfo.Id.Setter(principal, ident);
            }

            await genericRepository.SaveAsync(principal, cancellationToken);
        }

        return principal;
    }

    private async Task<TPrincipalIdent?> TryExtractIdent(string name, CancellationToken cancellationToken)
    {
        var tryCandidates = await userSources.Where(userSource => userSource.UserType != typeof(TPrincipal))
            .SyncWhenAll(userSource => userSource.ToSimple().TryGetUserAsync(name, cancellationToken));

        var identRequest =

            from tryUser in tryCandidates

            where tryUser is not null

            let ident = identityConverter.TryConvert(tryUser.Identity)

            where ident is not null

            select ident.Id;

        return identRequest.SingleOrDefault();
    }

    public async Task SaveAsync(TPrincipal principal, CancellationToken cancellationToken)
    {
        await this.ValidateAsync(principal, cancellationToken);

        await genericRepository.SaveAsync(principal, cancellationToken);
    }

    public async Task RemoveAsync(TPrincipal principal, bool force, CancellationToken cancellationToken)
    {
        if (!force && await queryableSource.GetQueryable<TPermission>()
                .GenericAnyAsync(permissionToPrincipalInfo.ToPrincipal.Path.Select(p => p == principal), cancellationToken))
        {
            throw new InvalidOperationException($"Removing principal \"{userSourceInfo.Name.Getter(principal)}\" must be empty");
        }

        await genericRepository.RemoveAsync(principal, cancellationToken);
    }

    public async Task ValidateAsync(TPrincipal principal, CancellationToken cancellationToken)
    {
        await principalGeneralValidator.ValidateAsync(principal, cancellationToken);
    }
}