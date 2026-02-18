using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using HierarchicalExpand;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;
using SecuritySystem.Validation;

namespace SecuritySystem.GeneralPermission.Validation.Permission;

public class PermissionDelegationValidator<TPermission, TPermissionRestriction>(
    IServiceProxyFactory serviceProxyFactory,
    IPermissionBindingInfoSource bindingInfoSource)
    : IPermissionValidator<TPermission, TPermissionRestriction>
{
    private readonly Lazy<IPermissionValidator<TPermission, TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var innerServiceType = typeof(PermissionDelegationValidator<,,>)
            .MakeGenericType(
                bindingInfo.PrincipalType,
                bindingInfo.PermissionType,
                typeof(TPermissionRestriction));

        return serviceProxyFactory.Create<IPermissionValidator<TPermission, TPermissionRestriction>>(innerServiceType, bindingInfo);
    });

    public Task ValidateAsync(PermissionData<TPermission, TPermissionRestriction> value, CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.ValidateAsync(value, cancellationToken);
}

public class PermissionDelegationValidator<TPrincipal, TPermission, TPermissionRestriction>(
    PermissionBindingInfo<TPermission, TPrincipal> permissionBindingInfo,
    IPermissionRestrictionRawConverter<TPermissionRestriction> permissionRestrictionRawConverter,
    IDomainObjectDisplayService domainObjectDisplayService,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityRoleSource securityRoleSource,
    IPermissionSecurityRoleResolver<TPermission> permissionSecurityRoleResolver,
    IQueryableSource queryableSource,
    IPermissionRestrictionLoader<TPermission, TPermissionRestriction> permissionRestrictionLoader,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory)
    : IPermissionValidator<TPermission, TPermissionRestriction>

    where TPrincipal : class
    where TPermission : class
{
    public async Task ValidateAsync(PermissionData<TPermission, TPermissionRestriction> permissionData, CancellationToken cancellationToken)
    {
        if (permissionBindingInfo.DelegatedFrom == null)
        {
            return;
        }

        var permission = permissionData.Permission;

        var delegatedFrom = permissionBindingInfo.DelegatedFrom.Getter(permission);

        if (delegatedFrom != null)
        {
            if (permissionBindingInfo.Principal.Getter(delegatedFrom) == permissionBindingInfo.Principal.Getter(permission))
            {
                throw new SecuritySystemValidationException("Permission cannot be delegated to the original user");
            }

            var delegatedFromData = await permissionRestrictionLoader.ToPermissionData(delegatedFrom, cancellationToken);

            this.ValidatePermissionDelegatedFrom(permissionData, delegatedFromData);
        }

        var subPermissions = await queryableSource
            .GetQueryable<TPermission>()
            .Where(permissionBindingInfo.DelegatedFrom.Path.Select(p => p == permission))
            .GenericToListAsync(cancellationToken);

        foreach (var subPermission in subPermissions)
        {
            var subPermissionData = await permissionRestrictionLoader.ToPermissionData(subPermission, cancellationToken);

            this.ValidatePermissionDelegatedFrom(subPermissionData, permissionData);
        }
    }

    private void ValidatePermissionDelegatedFrom(
        PermissionData<TPermission, TPermissionRestriction> subPermissionData,
        PermissionData<TPermission, TPermissionRestriction> delegatedFromData)
    {
        var subPermission = subPermissionData.Permission;
        var delegatedFrom = delegatedFromData.Permission;

        if (!this.IsCorrectRoleSubset(subPermission, delegatedFrom))
        {
            throw new SecuritySystemValidationException(
                $"Invalid delegated permission role. Selected role \"{permissionSecurityRoleResolver.Resolve(subPermission)}\" not subset of \"{permissionSecurityRoleResolver.Resolve(delegatedFrom)}\"");
        }

        if (!this.IsCorrectPeriodSubset(subPermission, delegatedFrom))
        {
            throw new SecuritySystemValidationException(
                $"Invalid delegated permission period. Selected period \"{permissionBindingInfo.GetSafePeriod(subPermission)}\" not subset of \"{permissionBindingInfo.GetSafePeriod(delegatedFrom)}\"");
        }

        {
            var invalidEntityGroups = this.GetInvalidContextDict(subPermissionData, delegatedFromData).ToList();

            if (invalidEntityGroups.Any())
            {
                throw new SecuritySystemValidationException(
                    string.Format(
                        "Can't delegate permission from {0} to {1}, because {0} have no access to objects ({2})",
                        domainObjectDisplayService.ToString(permissionBindingInfo.Principal.Getter(delegatedFrom)),
                        domainObjectDisplayService.ToString(permissionBindingInfo.Principal.Getter(subPermission)),
                        invalidEntityGroups.Join(
                            " | ",
                            g => $"{g.Key.Name}: {g.Value.OfType<object>().Join(", ")}")));
            }
        }
    }

    private bool IsCorrectRoleSubset(TPermission subPermission, TPermission delegatedFrom) =>

        permissionSecurityRoleResolver.Resolve(delegatedFrom)
            .GetAllElements(role => role.Information.Children.Select(securityRoleSource.GetSecurityRole))
            .Contains(permissionSecurityRoleResolver.Resolve(subPermission));

    private bool IsCorrectPeriodSubset(TPermission subPermission, TPermission delegatedFrom)
    {
        return permissionBindingInfo.GetSafePeriod(delegatedFrom).Contains(permissionBindingInfo.GetSafePeriod(subPermission));
    }

    private IEnumerable<KeyValuePair<SecurityContextInfo, Array>> GetInvalidContextDict(
        PermissionData<TPermission, TPermissionRestriction> subPermissionData,
        PermissionData<TPermission, TPermissionRestriction> delegatedFromData)
    {
        var subPermissionRestrictionDict = permissionRestrictionRawConverter.Convert(subPermissionData.Restrictions);

        var delegatedFromRestrictionDict = permissionRestrictionRawConverter.Convert(delegatedFromData.Restrictions);

        foreach (var securityContextInfo in securityContextInfoSource.SecurityContextInfoList)
        {
            var delegatedFromRestrictions = delegatedFromRestrictionDict.GetValueOrDefault(securityContextInfo.Type);

            if (delegatedFromRestrictions != null)
            {
                var subRestrictions = subPermissionRestrictionDict.GetValueOrDefault(securityContextInfo.Type);

                if (subRestrictions == null)
                {
                    yield return new(securityContextInfo, Array.Empty<object>());
                }
                else
                {
                    var expandedChildren = hierarchicalObjectExpanderFactory
                        .Create(securityContextInfo.Type)
                        .Expand(delegatedFromRestrictions, HierarchicalExpandType.Children);

                    var missedAccessElements = expandedChildren.OfType<object>().Except(subRestrictions.Cast<object>()).ToArray();

                    if (missedAccessElements.Length > 0)
                    {
                        yield return new (securityContextInfo, missedAccessElements);
                    }
                }
            }
        }
    }
}

