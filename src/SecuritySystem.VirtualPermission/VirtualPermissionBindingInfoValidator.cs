using CommonFramework;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionBindingInfoValidator(ISecurityRoleSource securityRoleSource) : IVirtualPermissionBindingInfoValidator
{
    private readonly HashSet<object> validated = new (ReferenceEqualityComparer.Instance);

    private readonly Lock locker = new ();

    public void Validate(VirtualPermissionBindingInfo virtualBindingInfo)
    {
        lock (this.locker)
        {
            if (this.validated.Contains(virtualBindingInfo))
            {
                return;
            }

            this.InternalValidate(virtualBindingInfo);

            this.validated.Add(virtualBindingInfo);
        }
    }

    private void InternalValidate(VirtualPermissionBindingInfo virtualBindingInfo)
    {
        var securityContextRestrictions = securityRoleSource
                                          .GetSecurityRole(virtualBindingInfo.SecurityRole)
                                          .Information
                                          .Restriction
                                          .SecurityContextRestrictions;

        if (securityContextRestrictions != null)
        {
            var bindingContextTypes = virtualBindingInfo.GetSecurityContextTypes().ToList();

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
