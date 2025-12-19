using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public class GeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole> : IGeneralPermissionSettings<TPermission, TSecurityRole>
    where TPermission : class
    where TSecurityRole : notnull
{
    private Expression<Func<TPermission, (DateTime StartDate, DateTime? EndDate)>>? periodPath;

    private Expression<Func<TPermission, string>>? commentPath;

    private Expression<Func<TSecurityRole, string>>? descriptionPath;

    private bool? isReadonly;




    public TGeneralPermissionBindingInfo ApplyOptionalPaths<TGeneralPermissionBindingInfo>(TGeneralPermissionBindingInfo bindingInfo)
        where TGeneralPermissionBindingInfo : GeneralPermissionBindingInfo<TPermission, TPrincipal, TSecurityRole>
    {
        return bindingInfo
            .PipeMaybe(this.periodPath, (b, v) => b with { PermissionPeriod = v.ToPropertyAccessors() })
            .PipeMaybe(this.commentPath, (b, v) => b with { PermissionComment = v.ToPropertyAccessors() })
            .PipeMaybe(this.descriptionPath, (b, v) => b with { SecurityRoleDescription = v.ToPropertyAccessors() })
            .PipeMaybe(this.isReadonly, (b, v) => b with { IsReadonly = v });
    }

    public IGeneralPermissionSettings<TPermission, TSecurityRole> SetPermissionPeriod(
        Expression<Func<TPermission, (DateTime StartDate, DateTime? EndDate)>> newPeriodPath)
    {
        this.periodPath = newPeriodPath;

        return this;
    }

    public IGeneralPermissionSettings<TPermission, TSecurityRole> SetPermissionComment(Expression<Func<TPermission, string>> newCommentPath)
    {
        this.commentPath = newCommentPath;

        return this;
    }

    public IGeneralPermissionSettings<TPermission, TSecurityRole> SetSecurityRoleDescription(Expression<Func<TSecurityRole, string>>? newDescriptionPath)
    {
        this.descriptionPath = newDescriptionPath;

        return this;
    }

    public IGeneralPermissionSettings<TPermission, TSecurityRole> SetReadonly(bool value = true)
    {
        this.isReadonly = value;

        return this;
    }
}