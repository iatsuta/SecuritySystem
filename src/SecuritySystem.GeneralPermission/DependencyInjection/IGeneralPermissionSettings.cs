using CommonFramework;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public interface IGeneralPermissionSettings<TPermission, TSecurityRole>
{
    public IGeneralPermissionSettings<TPermission, TSecurityRole> SetPermissionPeriod(
        PropertyAccessors<TPermission, DateTime?>? startDatePropertyAccessor,
        PropertyAccessors<TPermission, DateTime?>? endDatePropertyAccessor);

    public IGeneralPermissionSettings<TPermission, TSecurityRole> SetPermissionPeriod(
        Expression<Func<TPermission, DateTime?>>? startDatePath,
        Expression<Func<TPermission, DateTime?>>? endDatePath);

    IGeneralPermissionSettings<TPermission, TSecurityRole> SetPermissionComment(Expression<Func<TPermission, string>> commentPath);

    IGeneralPermissionSettings<TPermission, TSecurityRole> SetSecurityRoleDescription(Expression<Func<TSecurityRole, string>>? descriptionPath);

    IGeneralPermissionSettings<TPermission, TSecurityRole> SetReadonly(bool value = true);
}