using CommonFramework;

using GenericQueryable;

using Microsoft.Extensions.Logging;

using SecuritySystem.Services;

namespace SecuritySystem.TemplatePermission.Initialize;

public class TemplateSecurityRoleInitializer<TSecurityRole, TSecurityRoleIdent>(
    IQueryableSource queryableSource,
    IGenericRepository genericRepository,
    ISecurityRoleSource securityRoleSource,
    ILogger<TemplateSecurityRoleInitializer<TSecurityRole, TSecurityRoleIdent>> logger,
    IdentityInfo<TSecurityRole, TSecurityRoleIdent> identityInfo,
    SecurityRoleInfo<TSecurityRole> securityRoleInfo,
    ISecurityIdentityConverter<TSecurityRoleIdent> securityIdentityConverter,
	InitializerSettings settings)
    : ITemplateSecurityRoleInitializer<TSecurityRole>
	where TSecurityRole: class, new()
	where TSecurityRoleIdent : notnull
{
	public async Task<MergeResult<TSecurityRole, FullSecurityRole>> Init(CancellationToken cancellationToken)
    {
        return await this.Init(securityRoleSource.GetRealRoles(), cancellationToken);
    }

    public async Task<MergeResult<TSecurityRole, FullSecurityRole>> Init(
        IEnumerable<FullSecurityRole> securityRoles,
        CancellationToken cancellationToken)
    {
        var dbRoles = await queryableSource.GetQueryable<TSecurityRole>().GenericToListAsync(cancellationToken);

        var mergeResult = dbRoles.GetMergeResult(securityRoles, br => identityInfo.Id.Getter(br), sr => securityIdentityConverter.Convert(sr.Identity).Id);

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
                        logger.LogDebug("Role removed: {Name} {Id}", securityRoleInfo.Name.Getter(removingItem), identityInfo.Id.Getter(removingItem));

                        await genericRepository.RemoveAsync(removingItem, cancellationToken);
                    }

                    break;
                }
            }
        }

        foreach (var securityRole in mergeResult.AddingItems)
        {
	        var dbSecurityRole = new TSecurityRole();

	        securityRoleInfo.Name.Setter(dbSecurityRole, securityRole.Name);
	        securityRoleInfo.Description.Setter(dbSecurityRole, securityRole.Information.Description ?? "");
	        identityInfo.Id.Setter(dbSecurityRole, securityIdentityConverter.Convert(securityRole.Identity).Id);

            logger.LogDebug("Role created: {Name} {Id}", securityRole.Name, securityRole.Identity);

            await genericRepository.SaveAsync(securityRole, cancellationToken);
        }

        foreach (var (dbSecurityRole, securityRole) in mergeResult.CombineItems)
        {
            var newName = securityRole.Name;
            var newDescription = securityRole.Information.Description ?? "";

            if (newName != securityRoleInfo.Name.Getter(dbSecurityRole) || newDescription != securityRoleInfo.Description.Getter(dbSecurityRole))
			{
				securityRoleInfo.Name.Setter(dbSecurityRole, newName);
				securityRoleInfo.Description.Setter(dbSecurityRole, newDescription);

                logger.LogDebug("Role updated: {Name} {Description} {Id}", newName, newDescription, identityInfo.Id.Getter(dbSecurityRole));

                await genericRepository.SaveAsync(securityRole, cancellationToken);
            }
        }

        return mergeResult;
    }
    async Task ISecurityInitializer.Init(CancellationToken cancellationToken) => await this.Init(cancellationToken);
}
