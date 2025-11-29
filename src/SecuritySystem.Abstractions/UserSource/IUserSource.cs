using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public interface IUserSource<TUser>
{
	Task<TUser?> TryGetUserAsync(UserCredential userCredential, CancellationToken cancellationToken = default);

	Task<TUser> GetUserAsync(UserCredential userCredential, CancellationToken cancellationToken = default);

	TUser GetUser(UserCredential userCredential) => this.GetUserAsync(userCredential).GetAwaiter().GetResult();

    IUserSource<User> ToSimple();
}