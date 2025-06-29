﻿// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class SecurityRole(string name)
{
    public string Name { get; } = name;

    public override string ToString() => this.Name;

    public override bool Equals(object? obj) => this.Equals(obj as SecurityRole);

    protected bool Equals(SecurityRole? other) => other is not null && this.Name == other.Name;

    public override int GetHashCode() => this.Name.GetHashCode();


    public static bool operator ==(SecurityRole? sr1, SecurityRole? sr2) =>
        ReferenceEquals(sr1, sr2) || (sr1 is not null && sr1.Equals(sr2));

    public static bool operator !=(SecurityRole? sr1, SecurityRole? sr2) => !(sr1 == sr2);

    /// <summary>
    /// Администраторская роль
    /// </summary>
    public static SecurityRole Administrator { get; } = new(nameof(Administrator));

    /// <summary>
    /// Интеграционная роль
    /// </summary>
    public static SecurityRole SystemIntegration { get; } = new(nameof(SystemIntegration));
}
