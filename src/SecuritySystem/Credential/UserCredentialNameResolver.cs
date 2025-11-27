using CommonFramework;

namespace SecuritySystem.Credential;

public class RootUserCredentialNameResolver(IEnumerable<IUserCredentialNameByIdentityResolver> resolvers) : IUserCredentialNameResolver
{
	public virtual string GetUserName(UserCredential userCredential)
	{
		switch (userCredential)
		{
			case UserCredential.NamedUserCredential { Name: var name }:
				return name;

			case UserCredential.IdentUserCredential { Identity: var identity }:
			{
				var request = from resolver in resolvers

					let userName = resolver.TryGetUserName(identity)

					where userName != null

					select userName;

				return request.Distinct().Single(
					() => new Exception($"{nameof(UserCredential)} with {nameof(identity)} {identity} not found"),
					names => new Exception($"More one {nameof(UserCredential)} with {nameof(identity)} {identity}: {names.Join(", ", name => $"\"{name}\"")}"));
			}

			default: throw new ArgumentOutOfRangeException(nameof(userCredential));
		}
	}
}