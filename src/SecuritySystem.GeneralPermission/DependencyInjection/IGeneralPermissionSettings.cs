using CommonFramework;

using System.Linq.Expressions;

using SecuritySystem.GeneralPermission.Validation;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public interface IGeneralPermissionSettings<out TPrincipal, TPermission, TSecurityRole, TPermissionRestriction>
{
    public IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionPeriod(
        PropertyAccessors<TPermission, DateTime?>? startDatePropertyAccessor,
        PropertyAccessors<TPermission, DateTime?>? endDatePropertyAccessor);

    public IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionPeriod(
        Expression<Func<TPermission, DateTime?>>? startDatePath,
        Expression<Func<TPermission, DateTime?>>? endDatePath);

    IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionComment(
        Expression<Func<TPermission, string>> commentPath);

    IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetSecurityRoleDescription(
        Expression<Func<TSecurityRole, string>>? descriptionPath);

    IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetReadonly(bool value = true);

    IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionEqualityComparer<TComparer>()
        where TComparer : IPermissionEqualityComparer<TPermission, TPermissionRestriction>;

    IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionManagementService<TPermissionManagementService>()
        where TPermissionManagementService : IPermissionManagementService<TPrincipal, TPermission, TPermissionRestriction>;
}