using System.Collections.Immutable;

using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Testing;

public class TestPermission
{
    public TestPermission(SecurityRole securityRole)
        : this()
    {
        this.SecurityRole = securityRole;
    }

    public TestPermission()
    {
    }

    public SecurityRole? SecurityRole { get; set; }

    public PermissionPeriod Period { get; set; } = PermissionPeriod.Eternity;

    public Dictionary<Type, Array> Restrictions { get; } = [];

    public Dictionary<string, object> ExtendedData { get; set; } = [];

    public string Comment { get; set; } = "";

    public TypedSecurityIdentity<TIdent>? GetSingle<TSecurityContext, TIdent>()
        where TSecurityContext : ISecurityContext
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

    public ManagedPermissionData ToManagedPermissionData() => new()
    {
        SecurityRole = this.SecurityRole ?? throw new InvalidOperationException($"{nameof(this.SecurityRole)} not initialized"),
        Period = this.Period,
        Comment = this.Comment,
        Restrictions = this.Restrictions.Where(pair => pair.Value.Length > 0).ToImmutableDictionary(),
        ExtendedData = this.ExtendedData.ToImmutableDictionary()
    };

    public static implicit operator ManagedPermissionData(TestPermission testPermissionBuilder) => testPermissionBuilder.ToManagedPermissionData();
}