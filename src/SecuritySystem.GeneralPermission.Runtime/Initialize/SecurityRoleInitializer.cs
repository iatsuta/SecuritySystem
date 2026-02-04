using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Initialize;

public class SecurityRoleInitializer(IServiceProvider serviceProvider, IEnumerable<GeneralPermissionBindingInfo> bindings)
    : ISecurityRoleInitializer
{
    public async Task Initialize(CancellationToken cancellationToken)
    {
        foreach (var binding in bindings)
        {
            var initializer =
                (ISecurityRoleInitializer)serviceProvider.GetRequiredService(
                    typeof(ISecurityRoleInitializer<>).MakeGenericType(binding.SecurityRoleType));

            await initializer.Initialize(cancellationToken);
        }
    }
}

public class SecurityRoleInitializer<TSecurityRole>(
    IServiceProxyFactory serviceProxyFactory,
    IIdentityInfoSource identityInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IGeneralPermissionBindingInfoSource bindingInfoSource) : ISecurityRoleInitializer<TSecurityRole>
{
    private readonly Lazy<ISecurityRoleInitializer<TSecurityRole>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForSecurityRole(typeof(TSecurityRole));

        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityRole>();

        var visualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TSecurityRole>();

        var innerServiceType = typeof(SecurityRoleInitializer<,,>).MakeGenericType(
            bindingInfo.PermissionType,
            bindingInfo.SecurityRoleType,
            identityInfo.IdentityType);

        return serviceProxyFactory.Create<ISecurityRoleInitializer<TSecurityRole>>(
            innerServiceType,
            bindingInfo,
            identityInfo,
            visualIdentityInfo);
    });

    public Task<MergeResult<TSecurityRole, FullSecurityRole>> Initialize(IEnumerable<FullSecurityRole> securityRoles, CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.Initialize(securityRoles, cancellationToken);

    public Task<MergeResult<TSecurityRole, FullSecurityRole>> Initialize(CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.Initialize(cancellationToken);

    Task IInitializer.Initialize(CancellationToken cancellationToken) =>
        ((IInitializer)this.lazyInnerService.Value).Initialize(cancellationToken);
}

public class SecurityRoleInitializer<TPermission, TSecurityRole, TSecurityRoleIdent>(
    GeneralPermissionBindingInfo<TPermission, TSecurityRole> bindingInfo,
    IQueryableSource queryableSource,
    IGenericRepository genericRepository,
    ISecurityRoleSource securityRoleSource,
    ILogger<SecurityRoleInitializer<TPermission, TSecurityRole, TSecurityRoleIdent>> logger,
    IdentityInfo<TSecurityRole, TSecurityRoleIdent> identityInfo,
    VisualIdentityInfo<TSecurityRole> visualIdentityInfo,
    ISecurityIdentityConverter<TSecurityRoleIdent> securityIdentityConverter,
    InitializerSettings settings)
    : ISecurityRoleInitializer<TSecurityRole>
    where TSecurityRole : class, new()
    where TSecurityRoleIdent : notnull
{
    public async Task<MergeResult<TSecurityRole, FullSecurityRole>> Initialize(CancellationToken cancellationToken)
    {
        return await this.Initialize(securityRoleSource.GetRealRoles(), cancellationToken);
    }

    public async Task<MergeResult<TSecurityRole, FullSecurityRole>> Initialize(
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

    async Task IInitializer.Initialize(CancellationToken cancellationToken) => await this.Initialize(cancellationToken);
}