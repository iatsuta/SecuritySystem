using SecuritySystem.Credential;

using SecuritySystem.UserSource;

namespace SecuritySystem.Services;

public class RunAsManager<TUser>(
	IRawUserAuthenticationService rawUserAuthenticationService,
	ISecuritySystemFactory securitySystemFactory,
	IEnumerable<IRunAsValidator> validators,
	IUserSource<TUser> userSource,
	IUserSource<User> primitiveUserSource,
	IUserSourceRunAsAccessor<TUser> accessor,
	IGenericRepository genericRepository)
    : IRunAsManager
	where TUser : class
{
	private readonly Lazy<TUser> nativeCurrentUser = new(() =>
	{

	});

	private TUser NativeUser => this.nativeCurrentUser.Value;

	private TUser? NativeRunAsUser => accessor.GetRunAs(this.NativeUser);

	private UserCredential PureCredential { get; } = rawUserAuthenticationService.GetUserName();

	public User? RunAsUser { get; } = primitiveUserSource.GetUser(rawUserAuthenticationService.GetUserName());

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

    private async Task PersistRunAs(UserCredential? userCredential, CancellationToken cancellationToken)
	{
		var newRunAsUser = userCredential is null ? null : userSource.GetUser(userCredential);

		if (this.NativeRunAsUser != newRunAsUser)
		{
			accessor.SetRunAs(this.NativeUser, newRunAsUser);

			await genericRepository.SaveAsync(this.NativeUser, cancellationToken);
		}
	}

    private void CheckAccess() =>
        securitySystemFactory.Create(new SecurityRuleCredential.CurrentUserWithoutRunAsCredential())
                             .CheckAccess(SecurityRole.Administrator);
}

