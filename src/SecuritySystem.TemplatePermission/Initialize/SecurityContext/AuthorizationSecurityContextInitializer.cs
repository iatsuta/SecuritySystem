using CommonFramework;
using SecuritySystem.Attributes;

namespace SecuritySystem.TemplatePermission.Initialize;

public class TemplateSecurityContextInitializer(
    [DisabledSecurity] IRepository<TSecurityContextType> securityContextTypeRepository,
    ISecurityContextInfoSource securityContextInfoSource,
    ILogger<TemplateSecurityContextInitializer> logger,
    InitializerSettings settings)
    : ITemplateSecurityContextInitializer
{
    public async Task<MergeResult<TSecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken)
    {
        var dbSecurityContextTypes = await securityContextTypeRepository.GetQueryable().GenericToListAsync(cancellationToken);

        var mergeResult = dbSecurityContextTypes.GetMergeResult(securityContextInfoSource.SecurityContextInfoList, et => et.Id, sc => sc.Id);

        if (mergeResult.RemovingItems.Any())
        {
            switch (settings.UnexpectedAuthElementMode)
            {
                case UnexpectedAuthElementMode.RaiseError:
                    throw new InvalidOperationException(
                        $"Unexpected entity type in database: {mergeResult.RemovingItems.Join(", ")}");

                case UnexpectedAuthElementMode.Remove:
                {
                    foreach (var removingItem in mergeResult.RemovingItems)
                    {
                        logger.LogDebug("SecurityContextType removed: {Name} {Id}", removingItem.Name, removingItem.Id);

                        await securityContextTypeRepository.RemoveAsync(removingItem, cancellationToken);
                    }

                    break;
                }
            }
        }

        foreach (var securityContextInfo in mergeResult.AddingItems)
        {
            var securityContextType = new TSecurityContextType { Name = securityContextInfo.Name };

            logger.LogDebug("SecurityContextType created: {Name} {Id}", securityContextType.Name, securityContextType.Id);

            await securityContextTypeRepository.InsertAsync(securityContextType, securityContextInfo.Id, cancellationToken);
        }

        foreach (var (securityContextType, securityContextInfo) in mergeResult.CombineItems)
        {
            if (securityContextType.Name != securityContextInfo.Name)
            {
                securityContextType.Name = securityContextInfo.Name;

                logger.LogDebug("SecurityContextType updated: {Name} {Id}", securityContextInfo.Name, securityContextInfo.Id);

                await securityContextTypeRepository.SaveAsync(securityContextType, cancellationToken);
            }
        }

        return mergeResult;
    }

    async Task ISecurityInitializer.Init(CancellationToken cancellationToken) => await this.Init(cancellationToken);
}
