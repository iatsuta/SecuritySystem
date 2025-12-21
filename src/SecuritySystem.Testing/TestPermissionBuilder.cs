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

    //protected TIdentity? GetSingleIdentity<TIdentity>(Type type, Func<SecurityIdentity, TIdentity> map)
    //    where TIdentity : notnull
    //{
    //    return this.Restrictions.GetValueOrDefault(type) is [var v] ? v : null;
    //}

    //protected void SetSingleIdentity<TIdentity>(Type type, Func<TIdentity, SecurityIdentity> map, TIdentity? value)
    //    where TIdentity : struct
    //{
    //    if (value == null)
    //    {
    //        this.Restrictions[type] = [];
    //    }
    //    else
    //    {
    //        this.Restrictions[type] = new List<SecurityIdentity> { map(value.Value) };
    //    }
    //}

    //protected TIdentity GetSingleIdentityC<TIdentity>(Type type, Func<SecurityIdentity, TIdentity> map)
    //    where TIdentity : class
    //{
    //    return this.Restrictions.GetValueOrDefault(type).Maybe(v => map(v.Single()));
    //}

    //protected void SetSingleIdentityC<TIdentity>(Type type, Func<TIdentity, SecurityIdentity> map, TIdentity? value)
    //    where TIdentity : class
    //{
    //    if (value == null)
    //    {
    //        this.Restrictions[type] = new List<SecurityIdentity>();
    //    }
    //    else
    //    {
    //        this.Restrictions[type] = new List<SecurityIdentity> { map(value) };
    //    }
    //}


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