using CommonFramework;

using System.Linq.Expressions;

using SecuritySystem.GeneralPermission.Validation;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public interface IGeneralPermissionBuilder<out TPrincipal, TPermission, TSecurityRole, TPermissionRestriction>
{
    public IGeneralPermissionBuilder<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionPeriod(
        PropertyAccessors<TPermission, DateTime?>? startDatePropertyAccessor,
        PropertyAccessors<TPermission, DateTime?>? endDatePropertyAccessor);

    public IGeneralPermissionBuilder<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionPeriod(
        Expression<Func<TPermission, DateTime?>>? startDatePath,
        Expression<Func<TPermission, DateTime?>>? endDatePath);

    IGeneralPermissionBuilder<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionComment(
        Expression<Func<TPermission, string>> commentPath);

    IGeneralPermissionBuilder<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionDelegation(
        Expression<Func<TPermission, TPermission?>> delegatedFromPath);

    IGeneralPermissionBuilder<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetSecurityRoleDescription(
        Expression<Func<TSecurityRole, string>>? descriptionPath);

    IGeneralPermissionBuilder<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetReadonly(bool value = true);

    IGeneralPermissionBuilder<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionEqualityComparer<TComparer>()
        where TComparer : IPermissionEqualityComparer<TPermission, TPermissionRestriction>;

    IGeneralPermissionBuilder<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionManagementService<TPermissionManagementService>()
        where TPermissionManagementService : IPermissionManagementService<TPrincipal, TPermission, TPermissionRestriction>;
}