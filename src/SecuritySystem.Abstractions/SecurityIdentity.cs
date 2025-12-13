namespace SecuritySystem;

public abstract record SecurityIdentity
{
	public abstract Type IdentType { get; }

	public abstract object GetId();

	public static implicit operator SecurityIdentity(Guid id)
	{
		return new SecurityIdentity<Guid>(id);
	}

	public static implicit operator SecurityIdentity(int id)
	{
		return new SecurityIdentity<int>(id);
	}
}

public record SecurityIdentity<TIdent>(TIdent Id) : SecurityIdentity
	where TIdent : notnull
{
	public override Type IdentType { get; } = typeof(TIdent);

	public override object GetId()
	{
		return this.Id;
	}

	public override string? ToString() => this.Id.ToString();
}