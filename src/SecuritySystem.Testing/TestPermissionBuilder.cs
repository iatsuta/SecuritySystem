namespace SecuritySystem.Testing;

public class TestPermissionBuilder
{
    public TestPermissionBuilder(SecurityRole securityRole)
        : this()
    {
        this.SecurityRole = securityRole;
    }

    public TestPermissionBuilder()
    {
    }

    public SecurityRole? SecurityRole { get; set; }

    public PermissionPeriod Period { get; set; } = PermissionPeriod.Eternity;

    public Dictionary<Type, Array> Restrictions { get; } = new();

    public TypedSecurityIdentity<TIdent>? GetSingle<TSecurityContext, TIdent>()
        where TSecurityContext: ISecurityContext
        where TIdent : notnull
    {
        var arr = (TIdent[]?)this.Restrictions.GetValueOrDefault(typeof(TSecurityContext));

        var value = arr is null ? default : arr.Select(TIdent? (v) => v).SingleOrDefault();

        return value is null ? null : new TypedSecurityIdentity<TIdent>(value);
    }

    public void SetSingle<TSecurityContext, TIdent>(TypedSecurityIdentity<TIdent>? value)
        where TSecurityContext : ISecurityContext
        where TIdent : notnull
    {
        if (value == null)
        {
            this.Restrictions.Remove(typeof(TSecurityContext));
        }
        else
        {
            this.Restrictions[typeof(TSecurityContext)] = new[] { value.Id };
        }
    }

    public TypedSecurityIdentity<TIdent>[] GetMany<TSecurityContext, TIdent>()
        where TSecurityContext : ISecurityContext
        where TIdent : notnull
    {
        var arr = (TIdent[]?)this.Restrictions.GetValueOrDefault(typeof(TSecurityContext));

        var value = arr ?? Array.Empty<TIdent>();

        return value.Select(TypedSecurityIdentity.Create).ToArray();
    }

    public void SetMany<TSecurityContext, TIdent>(TypedSecurityIdentity<TIdent>[] value)
        where TSecurityContext : ISecurityContext
        where TIdent : notnull
    {
        if (value.Length == 0)
        {
            this.Restrictions.Remove(typeof(TSecurityContext));
        }
        else
        {
            this.Restrictions[typeof(TSecurityContext)] = value.Select(v => v.Id).ToArray();
        }
    }

    public static implicit operator TestPermission(TestPermissionBuilder testPermissionBuilder)
    {
        if (testPermissionBuilder.SecurityRole is null)
        {
            throw new InvalidOperationException($"{nameof(testPermissionBuilder.SecurityRole)} not initialized");
        }

        return new TestPermission(testPermissionBuilder.SecurityRole)
        {
            Period = testPermissionBuilder.Period,
            Restrictions = testPermissionBuilder.Restrictions.Where(pair => pair.Value.Length > 0).ToDictionary()
        };
    }
}