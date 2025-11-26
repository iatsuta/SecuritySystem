namespace SecuritySystem;

public interface ICurrentUser
{
	string Name { get; }

	SecurityIdentity Identity { get; }
}