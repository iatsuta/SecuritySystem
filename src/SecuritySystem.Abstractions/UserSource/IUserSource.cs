using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public interface IUserSource<TUser> : IUserSource
{
	Task<TUser?> TryGetUserAsync(UserCredential userCredential, CancellationToken cancellationToken = default);

	Task<TUser> GetUserAsync(UserCredential userCredential, CancellationToken cancellationToken = default);

	TUser GetUser(UserCredential userCredential) => this.GetUserAsync(userCredential).GetAwaiter().GetResult();

    TUser? TryGetUser(UserCredential userCredential) => this.TryGetUserAsync(userCredential).GetAwaiter().GetResult();
}

public interface IUserSource
{
    Type UserType { get; }

    IUserSource<User> ToSimple();
}