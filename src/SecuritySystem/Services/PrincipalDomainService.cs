using CommonFramework;
using CommonFramework.DependencyInjection;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;
using SecuritySystem.UserSource;

namespace SecuritySystem.Services;

public class PrincipalDomainService<TPrincipal>(
    IServiceProxyFactory serviceProxyFactory,
    IIdentityInfoSource identityInfoSource,
	IVisualIdentityInfoSource visualIdentityInfoSource,
    IPermissionBindingInfoSource bindingInfoSource) : IPrincipalDomainService<TPrincipal>
{
    private readonly Lazy<IPrincipalDomainService<TPrincipal>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPrincipal(typeof(TPrincipal));

        var identityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.PrincipalType);

        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo(bindingInfo.PrincipalType);

        var innerServiceType = typeof(PrincipalDomainService<,,>).MakeGenericType(
            bindingInfo.PrincipalType,
            bindingInfo.PermissionType,
            identityInfo.IdentityType);

        return serviceProxyFactory.Create<IPrincipalDomainService<TPrincipal>>(
            innerServiceType,
            bindingInfo,
            identityInfo,
            visualIdentityInfo);
    });

    private IPrincipalDomainService<TPrincipal> InnerService => this.lazyInnerService.Value;

    public Task<TPrincipal> GetOrCreateAsync(string name, CancellationToken cancellationToken) =>
        this.InnerService.GetOrCreateAsync(name, cancellationToken);

    public Task RemoveAsync(TPrincipal principal, bool force, CancellationToken cancellationToken) =>
        this.InnerService.RemoveAsync(principal, force, cancellationToken);
}

public class PrincipalDomainService<TPrincipal, TPermission, TPrincipalIdent>(
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    IQueryableSource queryableSource,
	IGenericRepository genericRepository,
	IEnumerable<IUserSource> userSources,
	ISecurityIdentityConverter<TPrincipalIdent> identityConverter,
	IdentityInfo<TPrincipal, TPrincipalIdent> identityInfo,
	VisualIdentityInfo<TPrincipal> visualIdentityInfo) : IPrincipalDomainService<TPrincipal>
	where TPrincipal : class, new()
	where TPrincipalIdent : notnull
	where TPermission : class
{
	public async Task<TPrincipal> GetOrCreateAsync(string name, CancellationToken cancellationToken)
	{
		var principal = await queryableSource.GetQueryable<TPrincipal>()
			.GenericSingleOrDefaultAsync(visualIdentityInfo.Name.Path.Select(ExpressionHelper.GetEqualityWithExpr(name)), cancellationToken);

		if (principal is null)
		{
			principal = new TPrincipal();

			visualIdentityInfo.Name.Setter(principal, name);

            await this.TryInitIdent(principal, cancellationToken);

			await genericRepository.SaveAsync(principal, cancellationToken);
		}

		return principal;
	}

    private async Task TryInitIdent(TPrincipal principal, CancellationToken cancellationToken)
    {
        var ident = await this.TryExtractIdent(visualIdentityInfo.Name.Getter(principal), cancellationToken);

        if (ident is not null)
        {
            identityInfo.Id.Setter(principal, ident);
        }
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

	public async Task RemoveAsync(TPrincipal principal, bool force, CancellationToken cancellationToken)
	{
		if (!force && await queryableSource.GetQueryable<TPermission>()
			    .GenericAnyAsync(bindingInfo.Principal.Path.Select(p => p == principal), cancellationToken))
		{
			throw new InvalidOperationException($"Removing principal \"{visualIdentityInfo.Name.Getter(principal)}\" must be empty");
		}

		await genericRepository.RemoveAsync(principal, cancellationToken);
	}
}