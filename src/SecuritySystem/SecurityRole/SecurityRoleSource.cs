using CommonFramework.DependencyInjection;

using SecuritySystem.Services;

using System.Collections.Concurrent;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class SecurityRoleSource : ISecurityRoleSource
{
    private readonly IReadOnlyDictionary<TypedSecurityIdentity, FullSecurityRole> identityDict;

    private readonly IReadOnlyDictionary<string, FullSecurityRole> nameDict;

    private readonly ConcurrentDictionary<SecurityIdentity, FullSecurityRole> baseIdentityCache = new();

    private readonly ISecurityIdentityConverter rootIdentityConverter;

    public SecurityRoleSource(IServiceProxyFactory serviceProxyFactory, IEnumerable<FullSecurityRole> securityRoles)
    {
        this.SecurityRoles = securityRoles.ToList();

        this.identityDict = this.SecurityRoles.ToDictionary(v => v.Identity);

        this.nameDict = this.SecurityRoles.ToDictionary(v => v.Name);

        this.rootIdentityConverter =
            serviceProxyFactory.Create<ISecurityIdentityConverter, RootSecurityIdentityConverter>(
                this.SecurityRoles.Select(sr => sr.Identity.IdentType).Distinct());
    }

    public IReadOnlyList<FullSecurityRole> SecurityRoles { get; }

    public FullSecurityRole GetSecurityRole(SecurityRole securityRole) => this.GetSecurityRole(securityRole.Name);

    public FullSecurityRole GetSecurityRole(string name) =>
        this.nameDict.GetValueOrDefault(name) ?? throw new ArgumentOutOfRangeException(nameof(name), $"{nameof(SecurityRole)} with name '{name}' not found");

    public FullSecurityRole GetSecurityRole(SecurityIdentity identity)
    {
        return this.baseIdentityCache.GetOrAdd(identity, _ =>
        {
            if (rootIdentityConverter.TryConvert(identity) is { } convertedIdentity && this.identityDict.TryGetValue(convertedIdentity, out var securityRole))
            {
                return securityRole;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(identity), $"SecurityRole with {nameof(identity)} '{identity}' not found");
            }
        });
    }

    public IEnumerable<FullSecurityRole> GetRealRoles()
    {
        return this.SecurityRoles.Where(sr => !sr.Information.IsVirtual);
    }
}