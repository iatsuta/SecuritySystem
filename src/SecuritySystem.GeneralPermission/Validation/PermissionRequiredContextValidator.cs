using CommonFramework;

namespace SecuritySystem.GeneralPermission.Validation;

public class PermissionRequiredContextValidator : ISecurityValidator<TPermission>
{
    public const string Key = "RequiredContext";

    public PermissionRequiredContextValidator(ISecurityContextInfoSource securityContextInfoSource, ISecurityRoleSource securityRoleSource)
    {
        this.RuleFor(permission => permission.Restrictions)
            .Must(
                (permission, _, context) =>
                {
                    var role = securityRoleSource.GetSecurityRole(permission.Role.Id);

                    if (role.Information.Restriction.SecurityContextRestrictions != null)
                    {
                        var requiredSecurityContextTypes = role.Information.Restriction.SecurityContextRestrictions
                                                               .Where(pair => pair.Required)
                                                               .Select(pair => pair.SecurityContextType);

                        var usedTypes = permission.Restrictions.Select(r => r.SecurityContextType).Distinct()
                                                  .Select(sct => securityContextInfoSource.GetSecurityContextInfo(sct.Id).Type);

                        var missedTypeInfoList = requiredSecurityContextTypes
                                                 .Except(usedTypes)
                                                 .Select(securityContextInfoSource.GetSecurityContextInfo)
                                                 .Select(info => info.Name)
                                                 .ToList();

                        context.MessageFormatter.AppendArgument("MissedTypes", missedTypeInfoList.Join(", "));

                        return missedTypeInfoList.Count == 0;
                    }
                    else
                    {
                        return true;
                    }
                })
            .WithMessage($"{nameof(TPermission)} must contain the required contexts: {{MissedTypes}}");
    }
}
