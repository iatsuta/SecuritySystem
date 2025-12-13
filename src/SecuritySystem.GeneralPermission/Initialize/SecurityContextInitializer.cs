using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Initialize;

public class SecurityContextInitializer<TSecurityContextType>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource) : ISecurityContextInitializer<TSecurityContextType>
{
    private readonly Lazy<ISecurityContextInitializer<TSecurityContextType>> lazyInnerService = new(() =>
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContextType>();

        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TSecurityContextType>();

        var innerServiceType = typeof(SecurityContextInitializer<,>).MakeGenericType(typeof(TSecurityContextType), identityInfo.IdentityType);

        return (ISecurityContextInitializer<TSecurityContextType>)ActivatorUtilities.CreateInstance(serviceProvider, innerServiceType, identityInfo, visualIdentityInfo);
    });

    public Task<MergeResult<TSecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.Init(cancellationToken);

    Task ISecurityInitializer.Init(CancellationToken cancellationToken) =>
        ((ISecurityInitializer)this.lazyInnerService.Value).Init(cancellationToken);
}

public class SecurityContextInitializer<TSecurityContextType, TSecurityContextTypeIdent>(
    IQueryableSource queryableSource,
    IGenericRepository genericRepository,
    ISecurityContextInfoSource securityContextInfoSource,
    ILogger<SecurityContextInitializer<TSecurityContextType, TSecurityContextTypeIdent>> logger,
    IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> identityInfo,
    VisualIdentityInfo<TSecurityContextType> visualIdentityInfo,
	ISecurityIdentityConverter<TSecurityContextTypeIdent> identityConverter,
    InitializerSettings settings)
    : ISecurityContextInitializer<TSecurityContextType>
	where TSecurityContextType : class, new()
	where TSecurityContextTypeIdent : notnull
{
    public async Task<MergeResult<TSecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken)
    {
	    var dbSecurityContextTypes = await queryableSource.GetQueryable<TSecurityContextType>().GenericToListAsync(cancellationToken);

	    var mergeResult = dbSecurityContextTypes.GetMergeResult(
		    securityContextInfoSource.SecurityContextInfoList,
		    identityInfo.Id.Getter,
		    sc => identityConverter.Convert(sc.Identity).Id);

        if (mergeResult.RemovingItems.Any())
        {
            switch (settings.UnexpectedSecurityElementMode)
            {
                case UnexpectedSecurityElementMode.RaiseError:
                    throw new InvalidOperationException(
                        $"Unexpected entity type in database: {mergeResult.RemovingItems.Join(", ")}");

                case UnexpectedSecurityElementMode.Remove:
                {
                    foreach (var removingItem in mergeResult.RemovingItems)
                    {
                        logger.LogDebug("SecurityContextType removed: {Name} {Id}", visualIdentityInfo.Name.Getter(removingItem), identityInfo.Id.Getter(removingItem));

                        await genericRepository.RemoveAsync(removingItem, cancellationToken);
                    }

                    break;
                }
            }
        }

        foreach (var securityContextInfo in mergeResult.AddingItems)
        {
	        var securityContextType = new TSecurityContextType();

	        visualIdentityInfo.Name.Setter(securityContextType, securityContextInfo.Name);
	        identityInfo.Id.Setter(securityContextType, identityConverter.Convert(securityContextInfo.Identity).Id);

			logger.LogDebug("SecurityContextType created: {Name} {Id}", visualIdentityInfo.Name.Getter(securityContextType), identityInfo.Id.Getter(securityContextType));

            await genericRepository.SaveAsync(securityContextType, cancellationToken);
        }

        foreach (var (securityContextType, securityContextInfo) in mergeResult.CombineItems)
        {
            if (visualIdentityInfo.Name.Getter(securityContextType) != securityContextInfo.Name)
            {
	            visualIdentityInfo.Name.Setter(securityContextType, securityContextInfo.Name);

                logger.LogDebug("SecurityContextType updated: {Name} {Id}", securityContextInfo.Name, identityInfo.Id.Getter(securityContextType));

                await genericRepository.SaveAsync(securityContextType, cancellationToken);
            }
        }

        return mergeResult;
    }

    async Task ISecurityInitializer.Init(CancellationToken cancellationToken) => await this.Init(cancellationToken);
}
