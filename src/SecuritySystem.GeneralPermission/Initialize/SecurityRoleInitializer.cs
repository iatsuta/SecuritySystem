using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SecuritySystem.Services;

using CommonFramework.VisualIdentitySource;

namespace SecuritySystem.GeneralPermission.Initialize;

public class SecurityRoleInitializer<TSecurityRole>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    GeneralPermissionBindingInfo bindingInfo) : ISecurityRoleInitializer<TSecurityRole>
{
    private readonly Lazy<ISecurityRoleInitializer<TSecurityRole>> lazyInnerService = new(() =>
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityRole>();

        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TSecurityRole>();

        var innerServiceType = typeof(SecurityRoleInitializer<,,,>).MakeGenericType(bindingInfo.PrincipalType, bindingInfo.PermissionType, typeof(TSecurityRole), identityInfo.IdentityType);

        return (ISecurityRoleInitializer<TSecurityRole>)ActivatorUtilities.CreateInstance(serviceProvider, innerServiceType, identityInfo, visualIdentityInfo);
    });

    public Task<MergeResult<TSecurityRole, FullSecurityRole>> Init(IEnumerable<FullSecurityRole> securityRoles, CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.Init(securityRoles, cancellationToken);

    public Task<MergeResult<TSecurityRole, FullSecurityRole>> Init(CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.Init(cancellationToken);

    Task ISecurityInitializer.Init(CancellationToken cancellationToken) =>
        ((ISecurityInitializer)this.lazyInnerService.Value).Init(cancellationToken);
}

public class SecurityRoleInitializer<TPrincipal, TPermission, TSecurityRole, TSecurityRoleIdent>(
    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole> bindingInfo,
    IQueryableSource queryableSource,
    IGenericRepository genericRepository,
    ISecurityRoleSource securityRoleSource,
    ILogger<SecurityRoleInitializer<TPrincipal, TPermission, TSecurityRole, TSecurityRoleIdent>> logger,
    IdentityInfo<TSecurityRole, TSecurityRoleIdent> identityInfo,
    VisualIdentityInfo<TSecurityRole> visualIdentityInfo,
    ISecurityIdentityConverter<TSecurityRoleIdent> securityIdentityConverter,
    InitializerSettings settings)
    : ISecurityRoleInitializer<TSecurityRole>
    where TSecurityRole : class, new()
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
            switch (settings.UnexpectedSecurityElementMode)
            {
                case UnexpectedSecurityElementMode.RaiseError:
                    throw new InvalidOperationException(
                        $"Unexpected roles in database: {mergeResult.RemovingItems.Join(", ")}");

                case UnexpectedSecurityElementMode.Remove:
                {
                    foreach (var removingItem in mergeResult.RemovingItems)
                    {
                        logger.LogDebug("Role removed: {Name} {Id}", visualIdentityInfo.Name.Getter(removingItem), identityInfo.Id.Getter(removingItem));

                        await genericRepository.RemoveAsync(removingItem, cancellationToken);
                    }

                    break;
                }
            }
        }

        foreach (var securityRole in mergeResult.AddingItems)
        {
            var dbSecurityRole = new TSecurityRole();

            visualIdentityInfo.Name.Setter(dbSecurityRole, securityRole.Name);
            bindingInfo.SecurityRoleDescription?.Setter(dbSecurityRole, securityRole.Information.Description ?? "");
            identityInfo.Id.Setter(dbSecurityRole, securityIdentityConverter.Convert(securityRole.Identity).Id);

            logger.LogDebug("Role created: {Name} {Id}", securityRole.Name, securityRole.Identity);

            await genericRepository.SaveAsync(dbSecurityRole, cancellationToken);
        }

        foreach (var (dbSecurityRole, securityRole) in mergeResult.CombineItems)
        {
            var newName = securityRole.Name;
            var newDescription = securityRole.Information.Description ?? "";

            if (newName != visualIdentityInfo.Name.Getter(dbSecurityRole) || (bindingInfo.SecurityRoleDescription != null &&
                                                                              newDescription != bindingInfo.SecurityRoleDescription.Getter(dbSecurityRole)))
            {
                visualIdentityInfo.Name.Setter(dbSecurityRole, newName);
                bindingInfo.SecurityRoleDescription?.Setter(dbSecurityRole, newDescription);

                logger.LogDebug("Role updated: {Name} {Description} {Id}", newName, newDescription, identityInfo.Id.Getter(dbSecurityRole));

                await genericRepository.SaveAsync(dbSecurityRole, cancellationToken);
            }
        }

        return mergeResult;
    }

    async Task ISecurityInitializer.Init(CancellationToken cancellationToken) => await this.Init(cancellationToken);
}