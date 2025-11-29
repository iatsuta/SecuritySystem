using CommonFramework;
using SecuritySystem.Credential;

using SecuritySystem.UserSource;

namespace SecuritySystem.Services;

public class RunAsManager<TUser>(
	IRawUserAuthenticationService rawUserAuthenticationService,
	ISecuritySystemFactory securitySystemFactory,
	IEnumerable<IRunAsValidator> validators,
	IUserSource<TUser> userSource,
	IUserSourceRunAsAccessor<TUser> accessor,
	IGenericRepository genericRepository,
	IDefaultUserConverter<TUser> toDefaultUserConverter)
	: IRunAsManager
	where TUser : class
{
	private readonly Lazy<TUser> lazyNativeCurrentUser = new(() => userSource.GetUser(rawUserAuthenticationService.GetUserName()));

	private TUser NativeCurrentUser => this.lazyNativeCurrentUser.Value;

	private TUser? NativeRunAsUser => accessor.GetRunAs(this.NativeCurrentUser);

	private UserCredential PureCredential { get; } = rawUserAuthenticationService.GetUserName();

	public User? RunAsUser => this.NativeRunAsUser?.Pipe(toDefaultUserConverter.ConvertFunc);

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
				await runAsValidator.ValidateAsync(userCredential, cancellationToken);
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
		var newRunAsUser = userCredential is null ? null : await userSource.GetUserAsync(userCredential, cancellationToken);

		if (this.NativeRunAsUser != newRunAsUser)
		{
			accessor.SetRunAs(this.NativeCurrentUser, newRunAsUser);

			await genericRepository.SaveAsync(this.NativeCurrentUser, cancellationToken);
		}
	}

	private void CheckAccess() =>
		securitySystemFactory.Create(new SecurityRuleCredential.CurrentUserWithoutRunAsCredential())
			.CheckAccess(SecurityRole.Administrator);
}