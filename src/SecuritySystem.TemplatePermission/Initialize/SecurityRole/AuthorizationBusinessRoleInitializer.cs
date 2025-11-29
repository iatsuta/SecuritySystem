using CommonFramework;

using GenericQueryable;

using Microsoft.Extensions.Logging;

using SecuritySystem.Services;

namespace SecuritySystem.TemplatePermission.Initialize;

public class AuthorizationBusinessRoleInitializer<TBusinessRole, TBusinessRoleIdent>(
    IQueryableSource queryableSource,
    IGenericRepository genericRepository,
    ISecurityRoleSource securityRoleSource,
    ILogger<AuthorizationBusinessRoleInitializer<TBusinessRole, TBusinessRoleIdent>> logger,
    IdentityInfo<TBusinessRole, TBusinessRoleIdent> identityInfo,
    BusinessRoleInfo<TBusinessRole> businessRoleInfo,
    ISecurityIdentityConverter<TBusinessRoleIdent> securityIdentityConverter,
	InitializerSettings settings)
    : IAuthorizationBusinessRoleInitializer<TBusinessRole>
	where TBusinessRole: class, new()
	where TBusinessRoleIdent : notnull
{
	public async Task<MergeResult<TBusinessRole, FullSecurityRole>> Init(CancellationToken cancellationToken)
    {
        return await this.Init(securityRoleSource.GetRealRoles(), cancellationToken);
    }

    public async Task<MergeResult<TBusinessRole, FullSecurityRole>> Init(
        IEnumerable<FullSecurityRole> securityRoles,
        CancellationToken cancellationToken)
    {
        var dbRoles = await queryableSource.GetQueryable<TBusinessRole>().GenericToListAsync(cancellationToken);

        var mergeResult = dbRoles.GetMergeResult(securityRoles, br => identityInfo.Getter(br), sr => securityIdentityConverter.Convert(sr.Identity).Id);

        if (mergeResult.RemovingItems.Any())
        {
            switch (settings.UnexpectedAuthElementMode)
            {
                case UnexpectedAuthElementMode.RaiseError:
                    throw new InvalidOperationException(
                        $"Unexpected roles in database: {mergeResult.RemovingItems.Join(", ")}");

                case UnexpectedAuthElementMode.Remove:
                {
                    foreach (var removingItem in mergeResult.RemovingItems)
                    {
                        logger.LogDebug("Role removed: {Name} {Id}", businessRoleInfo.NameGetter(removingItem), identityInfo.Getter(removingItem));

                        await genericRepository.RemoveAsync(removingItem, cancellationToken);
                    }

                    break;
                }
            }
        }

        foreach (var securityRole in mergeResult.AddingItems)
        {
	        var businessRole = new TBusinessRole();

	        businessRoleInfo.NameSetter(businessRole, securityRole.Name);
	        businessRoleInfo.DescriptionSetter(businessRole, securityRole.Information.Description ?? "");
	        identityInfo.Setter(businessRole, securityIdentityConverter.Convert(securityRole.Identity).Id);

            logger.LogDebug("Role created: {Name} {Id}", securityRole.Name, securityRole.Identity);

            await genericRepository.SaveAsync(businessRole, cancellationToken);
        }

        foreach (var (businessRole, securityRole) in mergeResult.CombineItems)
        {
            var newName = securityRole.Name;
            var newDescription = securityRole.Information.Description ?? "";

            if (newName != businessRoleInfo.NameGetter(businessRole) || newDescription != businessRoleInfo.DescriptionGetter(businessRole))
			{
				businessRoleInfo.NameSetter(businessRole, newName);
				businessRoleInfo.DescriptionSetter(businessRole, newDescription);

                logger.LogDebug("Role updated: {Name} {Description} {Id}", businessRole.Name, businessRole.Description, securityRole.Id);

                await businessRoleRepository.SaveAsync(businessRole, cancellationToken);
            }
        }

        return mergeResult;
    }
    async Task ISecurityInitializer.Init(CancellationToken cancellationToken) => await this.Init(cancellationToken);
}
