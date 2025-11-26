using CommonFramework;
using SecuritySystem.Attributes;
using SecuritySystem.Credential;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.TemplatePermission;

public class AuthorizationRunAsManager(
    IRawUserAuthenticationService userAuthenticationService,
    ISecuritySystemFactory securitySystemFactory,
    ICurrentPrincipalSource currentPrincipalSource,
    [DisabledSecurity] IRepository<TPrincipal> principalRepository,
    IPrincipalResolver principalResolver,
    IEnumerable<IRunAsValidator> validators)
    : RunAsManager(userAuthenticationService, securitySystemFactory)
{
    private TPrincipal CurrentPrincipal => currentPrincipalSource.CurrentPrincipal;

    public override User? RunAsUser =>
        this.CurrentPrincipal.RunAs.Maybe(runAsPrincipal => new User(runAsPrincipal.Id, runAsPrincipal.Name));

    protected override async Task PersistRunAs(UserCredential? userCredential, CancellationToken cancellationToken)
    {
        if (userCredential != null)
        {
            validators.Foreach(validator => validator.Validate(userCredential));
        }

        this.CurrentPrincipal.RunAs =
            userCredential == null
                ? null
                : await principalResolver.Resolve(userCredential, cancellationToken);

        await principalRepository.SaveAsync(this.CurrentPrincipal, cancellationToken);
    }
}
