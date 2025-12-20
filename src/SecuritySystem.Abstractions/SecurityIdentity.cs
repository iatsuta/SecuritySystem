namespace SecuritySystem;

public abstract record SecurityIdentity
{
    public abstract bool IsDefault { get; }

    public abstract object GetId();

    public static implicit operator SecurityIdentity(Guid id)
    {
        return (TypedSecurityIdentity)id;
    }

    public static implicit operator SecurityIdentity(int id)
    {
        return (TypedSecurityIdentity)id;
    }

    public override string? ToString() => this.GetId().ToString();

    public static SecurityIdentity Default { get; } = new UntypedSecurityIdentity("");
}

public record UntypedSecurityIdentity(string Id) : SecurityIdentity
{
    public override bool IsDefault => string.IsNullOrEmpty(this.Id);

    public override object GetId() => this.Id;
}

public abstract record TypedSecurityIdentity : SecurityIdentity
{
	public abstract Type IdentType { get; }


    public static implicit operator TypedSecurityIdentity(Guid id)
	{
		return Create(id);
	}

	public static implicit operator TypedSecurityIdentity(int id)
	{
		return Create(id);
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
}