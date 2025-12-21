using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public interface IGeneralPermissionSettings<TPermission, TSecurityRole>
{
    IGeneralPermissionSettings<TPermission, TSecurityRole> SetPermissionPeriod(
        Expression<Func<TPermission, PermissionPeriod>> periodPath);

    IGeneralPermissionSettings<TPermission, TSecurityRole> SetPermissionComment(Expression<Func<TPermission, string>> commentPath);

    IGeneralPermissionSettings<TPermission, TSecurityRole> SetSecurityRoleDescription(Expression<Func<TSecurityRole, string>>? descriptionPath);

    IGeneralPermissionSettings<TPermission, TSecurityRole> SetReadonly(bool value = true);
}