using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public interface IGeneralPermissionSettings<TPermission>
{
    IGeneralPermissionSettings<TPermission> AddPeriod(Expression<Func<(DateTime StartDate, DateTime? EndDate)>> periodPath);

    IGeneralPermissionSettings<TPermission> AddComment(Expression<Func<string>> commentPath);
}