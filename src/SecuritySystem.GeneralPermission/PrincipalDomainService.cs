using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.GeneralPermission;

public class PrincipalDomainService<TPrincipal>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
	IVisualIdentityInfoSource visualIdentityInfoSource,
    GeneralPermissionBindingInfo bindingInfo) : IPrincipalDomainService<TPrincipal>
{
    private readonly Lazy<IPrincipalDomainService<TPrincipal>> lazyInnerService = new(() =>
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TPrincipal>();

        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TPrincipal>();

        var innerServiceType = typeof(PrincipalDomainService<,,>).MakeGenericType(typeof(TPrincipal), bindingInfo.PermissionType, identityInfo.IdentityType);

        return (IPrincipalDomainService<TPrincipal>)ActivatorUtilities.CreateInstance(serviceProvider, innerServiceType, identityInfo, visualIdentityInfo);
    });

    private IPrincipalDomainService<TPrincipal> InnerService => this.lazyInnerService.Value;

    public Task<TPrincipal> GetOrCreateAsync(string name, CancellationToken cancellationToken) =>
        this.InnerService.GetOrCreateAsync(name, cancellationToken);

    //public Task SaveAsync(PrincipalData<TPrincipal> principal, CancellationToken cancellationToken) =>
    //    this.InnerService.SaveAsync(principal, cancellationToken);

    public Task RemoveAsync(TPrincipal principal, bool force, CancellationToken cancellationToken) =>
        this.InnerService.RemoveAsync(principal, force, cancellationToken);
}

public class PrincipalDomainService<TPrincipal, TPermission, TPrincipalIdent>(
	IQueryableSource queryableSource,
	IGenericRepository genericRepository,
	IEnumerable<IUserSource> userSources,
	IPermissionToPrincipalInfo<TPermission, TPrincipal> permissionToPrincipalInfo,
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

	//public async Task SaveAsync(PrincipalData<TPrincipal> basePrincipalData, CancellationToken cancellationToken)
 //   {
 //       var principal = basePrincipalData.Principal;

 //       if (identityInfo.Id.Getter(basePrincipalData.Principal) == default)
 //       {
 //           await this.TryInitIdent(principal, cancellationToken);
 //       }

 //       foreach (var permissionData in principalData.PermissionDataList)
 //       {
 //           foreach (var permissionRestriction in permissionData.Restrictions)
 //           {
 //               await genericRepository.SaveAsync(permissionRestriction, cancellationToken);
 //           }
 //       }

 //       var principalData = (PrincipalData<TPrincipal, TPermission, TPermissionRestriction>)basePrincipalData;

 //       await principalRootValidator.ValidateAsync(principalData, cancellationToken);

 //       await genericRepository.SaveAsync(principalData.Principal, cancellationToken);

 //       foreach (var permissionData in principalData.PermissionDataList)
 //       {
 //           await genericRepository.SaveAsync(permissionData.Permission, cancellationToken);

 //           foreach (var permissionRestriction in permissionData.Restrictions)
 //           {
 //               await genericRepository.SaveAsync(permissionRestriction, cancellationToken);
 //           }
 //       }
	//}

	public async Task RemoveAsync(TPrincipal principal, bool force, CancellationToken cancellationToken)
	{
		if (!force && await queryableSource.GetQueryable<TPermission>()
			    .GenericAnyAsync(permissionToPrincipalInfo.Principal.Path.Select(p => p == principal), cancellationToken))
		{
			throw new InvalidOperationException($"Removing principal \"{visualIdentityInfo.Name.Getter(principal)}\" must be empty");
		}

		await genericRepository.RemoveAsync(principal, cancellationToken);
	}
}