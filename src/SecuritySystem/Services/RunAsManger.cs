using CommonFramework;
using SecuritySystem.Credential;

using SecuritySystem.UserSource;

namespace SecuritySystem.Services;

public abstract class RunAsManager<TUser>(
	IRawUserAuthenticationService rawUserAuthenticationService,
	ISecuritySystemFactory securitySystemFactory,
	IEnumerable<IRunAsValidator> validators,
	IUserSource<User> baseUserSource,
	IUserSource<TUser> userSource,
	IUserSourceRunAsAccessor<TUser> accessor,
	IGenericRepository genericRepository)
    : IRunAsManager
{
	private readonly User currentUser = baseUserSource.GetUser(rawUserAuthenticationService.GetUserName());

	private TUser? NativeRunAsUser => accessor.GetRunAs(this.currentNativeUser);

    public User? RunAsUser =>

	private UserCredential PureCredential { get; } = rawUserAuthenticationService.GetUserName();

    public async Task StartRunAsUserAsync(UserCredential userCredential, CancellationToken cancellationToken)
    {
        this.CheckAccess();

        if (this.RunAsUser != null && userCredential.IsMatch(this.RunAsUser))
        {
        }
        else if (userCredential == this.PureCredential)
        {
            await this.FinishRunAsUserAsync(cancellationToken);
        }
        else
        {
			foreach (var runAsValidator in validators)
			{
				runAsValidator.Validate(userCredential);
			}

			await this.PersistRunAs(userCredential, cancellationToken);
        }
    }

    public async Task FinishRunAsUserAsync(CancellationToken cancellationToken)
    {
        this.CheckAccess();

        await this.PersistRunAs(null, cancellationToken);
    }

    private Task PersistRunAs(UserCredential? userCredential, CancellationToken cancellationToken)
	{
		var newRunAsUser = userCredential == null ? null : userSource.GetUser(userCredential);

		if (this.NativeRunAsUser != newRunAsUser)
		{
			accessor.SetRunAs(this.currentUser, newRunAsUser);

			await genericRepository.SaveAsync(this.currentUser, cancellationToken);
		}
	}

    private void CheckAccess() =>
        securitySystemFactory.Create(new SecurityRuleCredential.CurrentUserWithoutRunAsCredential())
                             .CheckAccess(SecurityRole.Administrator);

    User? IRunAsManager.RunAsUser => this.RunAsUser;
}
