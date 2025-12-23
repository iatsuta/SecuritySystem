using CommonFramework;
using CommonFramework.GenericRepository;

using SecuritySystem.Credential;

using SecuritySystem.UserSource;

namespace SecuritySystem.Services;

public class RunAsManager<TUser>(
	IRawUserAuthenticationService rawUserAuthenticationService,
	ISecuritySystemFactory securitySystemFactory,
	IEnumerable<IRunAsValidator> validators,
	IUserSource<TUser> userSource,
	UserSourceRunAsInfo<TUser> userSourceRunAsInfo,
	IGenericRepository genericRepository,
	IUserCredentialMatcher<TUser> userCredentialMatcher,
	IDefaultUserConverter<TUser> toDefaultUserConverter,
    ErrorMissedUserService<TUser> missedUserService) : IRunAsManager
	where TUser : class
{
	private readonly Lazy<TUser?> lazyNativeTryCurrentUser = new(() => userSource.TryGetUser(rawUserAuthenticationService.GetUserName()));

	private TUser? NativeTryCurrentUser => this.lazyNativeTryCurrentUser.Value;

    private TUser NativeCurrentUser => this.NativeTryCurrentUser ?? missedUserService.GetUser(rawUserAuthenticationService.GetUserName());

    private TUser? NativeRunAsUser => this.NativeTryCurrentUser == null ? null : userSourceRunAsInfo.RunAs.Getter(this.NativeTryCurrentUser);

	private UserCredential PureCredential { get; } = rawUserAuthenticationService.GetUserName();

	public User? RunAsUser => this.NativeRunAsUser?.Pipe(toDefaultUserConverter.ConvertFunc);

	public async Task StartRunAsUserAsync(UserCredential userCredential, CancellationToken cancellationToken)
	{
		this.CheckAccess();

		if (this.NativeRunAsUser is not null && userCredentialMatcher.IsMatch(userCredential, this.NativeRunAsUser))
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
			userSourceRunAsInfo.RunAs.Setter(this.NativeCurrentUser, newRunAsUser);

			await genericRepository.SaveAsync(this.NativeCurrentUser, cancellationToken);
		}
	}

	private void CheckAccess() =>
		securitySystemFactory.Create(new SecurityRuleCredential.CurrentUserWithoutRunAsCredential())
			.CheckAccess(SecurityRole.Administrator);
}