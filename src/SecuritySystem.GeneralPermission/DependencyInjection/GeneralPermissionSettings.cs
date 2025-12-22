using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public class GeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole> : IGeneralPermissionSettings<TPermission, TSecurityRole>
    where TPermission : class
    where TSecurityRole : notnull
{
    private PropertyAccessors<TPermission, DateTime?>? startDateAccessors;

    private PropertyAccessors<TPermission, DateTime?>? endDatedAccessors;

    private Expression<Func<TPermission, string>>? commentPath;

    private Expression<Func<TSecurityRole, string>>? descriptionPath;

    private bool? isReadonly;


    public TPermissionBindingInfo ApplyOptionalPaths<TPermissionBindingInfo>(TPermissionBindingInfo bindingInfo)
        where TPermissionBindingInfo : PermissionBindingInfo<TPermission, TPrincipal>
    {
        return bindingInfo
            .PipeMaybe(this.startDateAccessors, (b, v) => b with { PermissionStartDate = v })
            .PipeMaybe(this.endDatedAccessors, (b, v) => b with { PermissionEndDate = v })
            .PipeMaybe(this.commentPath, (b, v) => b with { PermissionComment = v.ToPropertyAccessors() })
            .PipeMaybe(this.isReadonly, (b, v) => b with { IsReadonly = v });
    }

    public TGeneralPermissionBindingInfo ApplyGeneralOptionalPaths<TGeneralPermissionBindingInfo>(TGeneralPermissionBindingInfo bindingInfo)
        where TGeneralPermissionBindingInfo : GeneralPermissionBindingInfo<TPermission, TSecurityRole>
    {
        return bindingInfo
            .PipeMaybe(this.descriptionPath, (b, v) => b with { SecurityRoleDescription = v.ToPropertyAccessors() });
    }

    public IGeneralPermissionSettings<TPermission, TSecurityRole> SetPermissionPeriod(
        PropertyAccessors<TPermission, DateTime?>? startDatePropertyAccessor,
        PropertyAccessors<TPermission, DateTime?>? endDatePropertyAccessor)
    {
        this.startDateAccessors = startDatePropertyAccessor;
        this.endDatedAccessors = endDatePropertyAccessor;

        return this;
    }

    public IGeneralPermissionSettings<TPermission, TSecurityRole> SetPermissionPeriod(
        Expression<Func<TPermission, DateTime?>>? startDatePath,
        Expression<Func<TPermission, DateTime?>>? endDatePath)
    {
        return this.SetPermissionPeriod(
            startDatePath == null ? null : new PropertyAccessors<TPermission, DateTime?>(startDatePath),
            endDatePath == null ? null : new PropertyAccessors<TPermission, DateTime?>(endDatePath));
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