using CommonFramework;
using CommonFramework.GenericRepository;

using GenericQueryable;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

using System.Collections.Immutable;

namespace SecuritySystem.GeneralPermission;

public class PermissionManagementService<TPrincipal, TPermission, TPermissionRestriction>(
    IServiceProxyFactory serviceProxyFactory,
    IPermissionBindingInfoSource bindingInfoSource,
    IGeneralPermissionBindingInfoSource generalBindingInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource)
    : IPermissionManagementService<TPrincipal, TPermission, TPermissionRestriction>
{
    private readonly Lazy<IPermissionManagementService<TPrincipal, TPermission, TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var generalBindingInfo = generalBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var innerServiceType = typeof(PermissionManagementService<,,,,,>)
            .MakeGenericType(
                bindingInfo.PrincipalType,
                bindingInfo.PermissionType,
                generalBindingInfo.SecurityRoleType,
                restrictionBindingInfo.PermissionRestrictionType,
                restrictionBindingInfo.SecurityContextTypeType,
                restrictionBindingInfo.SecurityContextObjectIdentType);

        return serviceProxyFactory.Create<IPermissionManagementService<TPrincipal, TPermission, TPermissionRestriction>>(
            innerServiceType,
            bindingInfo,
            generalBindingInfo,
            restrictionBindingInfo);
    });

    private IPermissionManagementService<TPrincipal, TPermission, TPermissionRestriction> InnerService => this.lazyInnerService.Value;

    public virtual Task<ManagedPermission> ToManagedPermissionAsync(TPermission dbPermission, CancellationToken cancellationToken) =>
        this.InnerService.ToManagedPermissionAsync(dbPermission, cancellationToken);

    public virtual Task<PermissionData<TPermission, TPermissionRestriction>> CreatePermissionAsync(TPrincipal dbPrincipal, ManagedPermission managedPermission, CancellationToken cancellationToken) =>
        this.InnerService.CreatePermissionAsync(dbPrincipal, managedPermission, cancellationToken);

    public virtual Task<(PermissionData<TPermission, TPermissionRestriction> PermissonData, bool Updated)> UpdatePermission(TPermission dbPermission,
        ManagedPermission managedPermission, CancellationToken cancellationToken) =>
        this.InnerService.UpdatePermission(dbPermission, managedPermission, cancellationToken);
}

public class PermissionManagementService<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    GeneralPermissionBindingInfo<TPermission, TSecurityRole> generalBindingInfo,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission> restrictionBindingInfo,

    IPermissionSecurityRoleResolver<TPermission> permissionSecurityRoleResolver,
    IRawPermissionRestrictionLoader<TPermission> rawPermissionRestrictionLoader,
    ISecurityIdentityExtractor<TPermission> permissionSecurityIdentityExtractor,
    ISecurityRepository<TSecurityRole> securityRoleRepository,
    IQueryableSource queryableSource,
    ISecurityRoleSource securityRoleSource,
    ISecurityRepository<TSecurityContextType> securityContextTypeRepository,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityIdentityExtractor<TSecurityRole> securityRoleIdentityExtractor,
    ISecurityIdentityExtractor<TSecurityContextType> securityContextTypeIdentityExtractor,
    ISecurityIdentityExtractor<TPermission> permissionIdentityExtractor,
    IGenericRepository genericRepository,
    ISecurityRepository<TPermission> permissionRepository)
    : IPermissionManagementService<TPrincipal, TPermission, TPermissionRestriction>

    where TPermission : class, new()
    where TSecurityRole : class
    where TPermissionRestriction : class, new()
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
{
    public async Task<ManagedPermission> ToManagedPermissionAsync(TPermission dbPermission, CancellationToken cancellationToken) =>
        new()
        {
            Identity = permissionSecurityIdentityExtractor.Extract(dbPermission),
            IsVirtual = bindingInfo.IsReadonly,
            SecurityRole = permissionSecurityRoleResolver.Resolve(dbPermission),
            Period = bindingInfo.GetSafePeriod(dbPermission),
            Comment = bindingInfo.GetSafeComment(dbPermission),
            DelegatedFrom = bindingInfo.DelegatedFrom?.Getter.Invoke(dbPermission) is { } delegatedFromPermission
                ? permissionIdentityExtractor.Extract(delegatedFromPermission)
                : SecurityIdentity.Default,
            Restrictions = (await rawPermissionRestrictionLoader.LoadAsync(dbPermission, cancellationToken)).ToImmutableDictionary()
        };

    public async Task<PermissionData<TPermission, TPermissionRestriction>> CreatePermissionAsync(
        TPrincipal dbPrincipal,
        ManagedPermission managedPermission,
        CancellationToken cancellationToken)
    {
        if (!managedPermission.Identity.IsDefault || managedPermission.IsVirtual)
        {
            throw new SecuritySystemException("wrong typed permission");
        }

        var securityRole = securityRoleSource.GetSecurityRole(managedPermission.SecurityRole);

        var dbRole = await securityRoleRepository.GetObjectAsync(securityRole.Identity, cancellationToken);

        var newDbPermission = new TPermission();

        bindingInfo.Principal.Setter(newDbPermission, dbPrincipal);
        generalBindingInfo.SecurityRole.Setter(newDbPermission, dbRole);

        bindingInfo.PermissionStartDate?.Setter(newDbPermission, managedPermission.Period.StartDate);
        bindingInfo.PermissionEndDate?.Setter(newDbPermission, managedPermission.Period.EndDate);
        bindingInfo.PermissionComment?.Setter(newDbPermission, managedPermission.Comment);

        if (!managedPermission.DelegatedFrom.IsDefault)
        {
            var delegatedFromAccessors = bindingInfo.DelegatedFrom ?? throw new InvalidOperationException("Delegated Permission Binding not initialized");

            var delegatedFromPermission = await permissionRepository.GetObjectAsync(managedPermission.DelegatedFrom, cancellationToken);

            delegatedFromAccessors.Setter(newDbPermission, delegatedFromPermission);
        }

        await genericRepository.SaveAsync(newDbPermission, cancellationToken);

        var newPermissionRestrictions = await managedPermission.Restrictions.SyncWhenAll(async restrictionGroup =>
        {
            var securityContextTypeIdentity = securityContextInfoSource.GetSecurityContextInfo(restrictionGroup.Key).Identity;

            var dbSecurityContextType = await securityContextTypeRepository.GetObjectAsync(securityContextTypeIdentity, cancellationToken);

            var newPermissionRestrictions = await restrictionGroup.Value.Cast<TSecurityContextObjectIdent>().SyncWhenAll(async securityContextId =>
            {
                var newDbPermissionRestriction = new TPermissionRestriction();

                restrictionBindingInfo.Permission.Setter(newDbPermissionRestriction, newDbPermission);
                restrictionBindingInfo.SecurityContextObjectId.Setter(newDbPermissionRestriction, securityContextId);
                restrictionBindingInfo.SecurityContextType.Setter(newDbPermissionRestriction, dbSecurityContextType);

                await genericRepository.SaveAsync(newDbPermissionRestriction, cancellationToken);

                return newDbPermissionRestriction;
            });

            return newPermissionRestrictions;
        });

        return new PermissionData<TPermission, TPermissionRestriction>(newDbPermission, newPermissionRestrictions.SelectMany());
    }

    public async Task<(PermissionData<TPermission, TPermissionRestriction> PermissonData, bool Updated)> UpdatePermission(
        TPermission dbPermission,
        ManagedPermission managedPermission,
        CancellationToken cancellationToken)
    {
        if (managedPermission.Identity.IsDefault || managedPermission.IsVirtual)
        {
            throw new SecuritySystemException("wrong typed permission");
        }

        if (!managedPermission.DelegatedFrom.IsDefault)
        {
            var delegatedFromAccessors = bindingInfo.DelegatedFrom ?? throw new InvalidOperationException("Delegated Permission Binding not initialized");

            var delegatedFromPermission = await permissionRepository.GetObjectAsync(managedPermission.DelegatedFrom, cancellationToken);

            if (delegatedFromPermission != delegatedFromAccessors.Getter(dbPermission))
            {
                throw new InvalidOperationException("Delegated source can't be changed");
            }
        }

        var securityRole = generalBindingInfo
            .SecurityRole
            .Getter(dbPermission)
            .Pipe(securityRoleIdentityExtractor.Extract)
            .Pipe(securityRoleSource.GetSecurityRole);

        if (securityRole != managedPermission.SecurityRole)
        {
            throw new SecuritySystemException("Permission role can't be changed");
        }

        var dbRestrictions = await queryableSource.GetQueryable<TPermissionRestriction>()
            .Where(restrictionBindingInfo.Permission.Path.Select(p => p == dbPermission))
            .GenericToListAsync(cancellationToken);

        var restrictionMergeResult = dbRestrictions.GetMergeResult(
            managedPermission.Restrictions
                .ChangeKey(t => securityContextInfoSource.GetSecurityContextInfo(t).Identity)
                .SelectMany(pair => pair.Value.Cast<TSecurityContextObjectIdent>().Select(securityContextId => (pair.Key, securityContextId))),
            pr => (
                securityContextTypeIdentityExtractor.Extract(restrictionBindingInfo.SecurityContextType.Getter(pr)),
                restrictionBindingInfo.SecurityContextObjectId.Getter(pr)),
            pair => pair);

        if (restrictionMergeResult.IsEmpty
            && (bindingInfo.PermissionComment == null || bindingInfo.PermissionComment.Getter(dbPermission) == managedPermission.Comment)
            && (bindingInfo.PermissionStartDate == null || bindingInfo.PermissionStartDate.Getter(dbPermission) == managedPermission.Period.StartDate)
            && (bindingInfo.PermissionEndDate == null || bindingInfo.PermissionEndDate.Getter(dbPermission) == managedPermission.Period.EndDate))
        {
            var permissionData = new PermissionData<TPermission, TPermissionRestriction>(dbPermission,
                restrictionMergeResult.CombineItems.Select(v => v.Item1));

            return (permissionData, false);
        }
        else
        {
            bindingInfo.PermissionComment?.Setter.Invoke(dbPermission, managedPermission.Comment);
            bindingInfo.PermissionStartDate?.Setter.Invoke(dbPermission, managedPermission.Period.StartDate);
            bindingInfo.PermissionEndDate?.Setter.Invoke(dbPermission, managedPermission.Period.EndDate);

            var newPermissionRestrictions = await restrictionMergeResult.AddingItems.SyncWhenAll(async restriction =>
            {
                var newPermissionRestriction = new TPermissionRestriction();

                var dbSecurityContextType =
                    await securityContextTypeRepository.GetObjectAsync(restriction.Key, cancellationToken);

                restrictionBindingInfo.Permission.Setter(newPermissionRestriction, dbPermission);
                restrictionBindingInfo.SecurityContextObjectId.Setter(newPermissionRestriction, restriction.securityContextId);
                restrictionBindingInfo.SecurityContextType.Setter(newPermissionRestriction, dbSecurityContextType);

                await genericRepository.SaveAsync(newPermissionRestriction, cancellationToken);

                return newPermissionRestriction;
            });

            foreach (var dbRestriction in restrictionMergeResult.RemovingItems)
            {
                await genericRepository.RemoveAsync(dbRestriction, cancellationToken);
            }

            var permissionData = new PermissionData<TPermission, TPermissionRestriction>(dbPermission,
                restrictionMergeResult.CombineItems.Select(v => v.Item1).Concat(newPermissionRestrictions));

            await genericRepository.SaveAsync(dbPermission, cancellationToken);

            return (permissionData, true);
        }
    }
}