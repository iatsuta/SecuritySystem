namespace SecuritySystem;

public record DisplayObjectInfo<TDomainObject>(Func<TDomainObject, string> DisplayFunc) : DisplayObjectInfo
	where TDomainObject : class
{
	public override Type DomainObjectType { get; } = typeof(TDomainObject);

	public static DisplayObjectInfo<TDomainObject> Default { get; } = new(v => v.ToString() ?? typeof(TDomainObject).Name);
}

public abstract record DisplayObjectInfo
{
	public abstract Type DomainObjectType { get; }
}