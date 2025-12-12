using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public interface IGeneralPermissionSettings<TPermission>
{
    IGeneralPermissionSettings<TPermission> SetPeriod(Expression<Func<TPermission, (DateTime StartDate, DateTime? EndDate)>> periodPath);

    IGeneralPermissionSettings<TPermission> SetComment(Expression<Func<TPermission, string>> commentPath);
}