namespace SecuritySystem.Credential;

public abstract record UserCredential
{
	public record NamedUserCredential(string Name) : UserCredential
	{
		public override string ToString() => this.Name;
	}

	public record IdentUserCredential(SecurityIdentity Identity) : UserCredential
	{
		public override string? ToString() => this.Identity.ToString();
	}


	public static implicit operator UserCredential(string? name)
	{
		return name == null ? null! : new NamedUserCredential(name);
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