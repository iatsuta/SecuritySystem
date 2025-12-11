using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public class GeneralPermissionSettings<TPrincipal, TPermission>
    : IGeneralPermissionSettings<TPermission>
    where TPermission : class
{
    public Expression<Func<TPermission, (DateTime StartDate, DateTime? EndDate)>>? PeriodPath { get; private set; }

    public Expression<Func<TPermission, string>>? CommentPath { get; private set; }


    public GeneralPermissionBindingInfo ApplyOptionalPaths(GeneralPermissionBindingInfo<TPrincipal, TPermission> bindingInfo)
    {
        return bindingInfo
            .PipeMaybe(this.PeriodPath, (b, v) => b with { Period = v.ToPropertyAccessors() })
            .PipeMaybe(this.CommentPath, (b, v) => b with { Comment = v.ToPropertyAccessors() });
    }

    public IGeneralPermissionSettings<TPermission> SetPeriod(Expression<Func<TPermission, (DateTime StartDate, DateTime? EndDate)>> periodPath)
    {
        this.PeriodPath = periodPath;

        return this;
    }

    public IGeneralPermissionSettings<TPermission> SetComment(Expression<Func<TPermission, string>> commentPath)
    {
        this.CommentPath = commentPath;

        return this;
    }
}