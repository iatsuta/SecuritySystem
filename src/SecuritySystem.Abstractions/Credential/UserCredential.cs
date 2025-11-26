using SecuritySystem.UserSource;

namespace SecuritySystem.Credential;

public abstract record UserCredential
{
	public abstract bool IsMatch(User user);

	public record NamedUserCredential(string Name) : UserCredential
	{
		public override bool IsMatch(User user) => this.IsMatch(user.Name);

		public override int GetHashCode() => this.Name.ToLower().GetHashCode();

		public virtual bool Equals(NamedUserCredential? other) => other is not null && this.IsMatch(other.Name);

		public override string ToString() => this.Name;

		private bool IsMatch(string name) => string.Equals(name, this.Name, StringComparison.OrdinalIgnoreCase);
	}

	public record IdentUserCredential(SecurityIdentity Identity) : UserCredential
	{
		public override bool IsMatch(User user) => user.Identity == this.Identity;

		public override string? ToString() => this.Identity.ToString();
	}

	public static implicit operator UserCredential(string name)
	{
		return name == null ? null : new NamedUserCredential(name);
	}

	public static implicit operator UserCredential(SecurityIdentity identity)
	{
		return new IdentUserCredential(identity);
	}

	public static implicit operator UserCredential(Guid id)
	{
		return (SecurityIdentity)id;
	}

	public static implicit operator UserCredential(int id)
	{
		return (SecurityIdentity)id;
	}
}