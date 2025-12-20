using CommonFramework;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionBindingInfoValidator(ISecurityRoleSource securityRoleSource) : IVirtualPermissionBindingInfoValidator
{
    private readonly HashSet<object> validated = new (ReferenceEqualityComparer.Instance);

    private readonly Lock locker = new ();

    public void Validate<TPrincipal, TPermission>(VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo)
    {
        lock (this.locker)
        {
            if (this.validated.Contains(bindingInfo))
            {
                return;
            }

            this.InternalValidate(bindingInfo);

            this.validated.Add(bindingInfo);
        }
    }

    private void InternalValidate<TPrincipal, TPermission>(VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo)
    {
        var securityContextRestrictions = securityRoleSource
                                          .GetSecurityRole(bindingInfo.SecurityRole)
                                          .Information
                                          .Restriction
                                          .SecurityContextRestrictions;

        if (securityContextRestrictions != null)
        {
            var bindingContextTypes = bindingInfo.GetSecurityContextTypes().ToList();

            var invalidTypes = bindingContextTypes.Except(securityContextRestrictions.Select(r => r.SecurityContextType)).ToList();

            if (invalidTypes.Any())
            {
                throw new Exception($"Invalid restriction types: {invalidTypes.Join(", ", t => t.Name)}");
            }

            var missedTypes = securityContextRestrictions
                              .Where(r => r.Required)
                              .Select(r => r.SecurityContextType)
                              .Except(bindingContextTypes)
                              .ToList();

            if (missedTypes.Any())
            {
                throw new Exception($"Missed required restriction types: {missedTypes.Join(", ", t => t.Name)}");
            }
        }
    }
}
