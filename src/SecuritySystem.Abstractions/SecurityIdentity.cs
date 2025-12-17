namespace SecuritySystem;

public abstract record SecurityIdentity
{
	public abstract bool IsDefault { get; }

    public abstract object GetId();

    public static implicit operator SecurityIdentity(Guid id)
    {
        return (TypedSecurityIdentity) id;
    }

    public static implicit operator SecurityIdentity(int id)
    {
        return (TypedSecurityIdentity)id;
    }
}

public record UntypedSecurityIdentity(string Id) : SecurityIdentity
{
    public override bool IsDefault => EqualityComparer<string>.Default.Equals(this.Id, null);

    public override object GetId() => this.Id;
}

public abstract record TypedSecurityIdentity : SecurityIdentity
{
	public abstract Type IdentType { get; }

	public static implicit operator TypedSecurityIdentity(Guid id)
	{
		return new TypedSecurityIdentity<Guid>(id);
	}

	public static implicit operator TypedSecurityIdentity(int id)
	{
		return new TypedSecurityIdentity<int>(id);
	}

    public static TypedSecurityIdentity<TIdent> Create<TIdent>(TIdent ident)
        where TIdent : notnull
    {
        return new TypedSecurityIdentity<TIdent>(ident);
    }
}

public record TypedSecurityIdentity<TIdent>(TIdent Id) : TypedSecurityIdentity
	where TIdent : notnull
{
    public override bool IsDefault => EqualityComparer<TIdent>.Default.Equals(this.Id, default);

    public override Type IdentType { get; } = typeof(TIdent);

	public override object GetId()
	{
		return this.Id;
	}

	public override string? ToString() => this.Id.ToString();
}