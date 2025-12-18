using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public class GeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole> : IGeneralPermissionSettings<TPermission, TSecurityRole>
    where TPermission : class
    where TSecurityRole : notnull
{
    public Expression<Func<TPermission, (DateTime StartDate, DateTime? EndDate)>>? PeriodPath { get; private set; }

    public Expression<Func<TPermission, string>>? CommentPath { get; private set; }

    public Expression<Func<TSecurityRole, string>>? DescriptionPath { get; private set; }


    public TGeneralPermissionBindingInfo ApplyOptionalPaths<TGeneralPermissionBindingInfo>(TGeneralPermissionBindingInfo bindingInfo)
        where TGeneralPermissionBindingInfo : GeneralPermissionBindingInfo<TPermission, TPrincipal, TSecurityRole>
    {
        return bindingInfo
            .PipeMaybe(this.PeriodPath, (b, v) => b with { PermissionPeriod = v.ToPropertyAccessors() })
            .PipeMaybe(this.CommentPath, (b, v) => b with { PermissionComment = v.ToPropertyAccessors() })
            .PipeMaybe(this.DescriptionPath, (b, v) => b with { SecurityRoleDescription = v.ToPropertyAccessors() });
    }

    public IGeneralPermissionSettings<TPermission, TSecurityRole> SetPermissionPeriod(
        Expression<Func<TPermission, (DateTime StartDate, DateTime? EndDate)>> periodPath)
    {
        this.PeriodPath = periodPath;

        return this;
    }

    public IGeneralPermissionSettings<TPermission, TSecurityRole> SetPermissionComment(Expression<Func<TPermission, string>> commentPath)
    {
        this.CommentPath = commentPath;

        return this;
    }

    public IGeneralPermissionSettings<TPermission, TSecurityRole> SetSecurityRoleDescription(Expression<Func<TSecurityRole, string>>? descriptionPath)
    {
        this.DescriptionPath = descriptionPath;

        return this;
    }
}