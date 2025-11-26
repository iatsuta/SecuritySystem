namespace SecuritySystem;

public abstract record SecurityIdentity
{
	public static implicit operator SecurityIdentity(Guid id)
	{
		return new SecurityIdentity<Guid>(id);
	}

	public static implicit operator SecurityIdentity(int id)
	{
		return new SecurityIdentity<int>(id);
	}
}

public record SecurityIdentity<T>(T Id) : SecurityIdentity
{
	public override string? ToString() => this.Id?.ToString();
}