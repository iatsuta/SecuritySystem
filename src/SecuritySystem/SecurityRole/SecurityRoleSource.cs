using CommonFramework;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class SecurityRoleSource : ISecurityRoleSource
{
	private readonly IReadOnlyDictionary<SecurityIdentity, FullSecurityRole> identityDict;

	private readonly IReadOnlyDictionary<string, FullSecurityRole> nameDict;

	public SecurityRoleSource(IEnumerable<FullSecurityRole> securityRoles)
	{
		this.SecurityRoles = securityRoles.ToList();

		this.Validate();

		this.identityDict = this.SecurityRoles.ToDictionary(v => v.Identity);

		this.nameDict = this.SecurityRoles.ToDictionary(v => v.Name);
	}

	public IReadOnlyList<FullSecurityRole> SecurityRoles { get; }

	public FullSecurityRole GetSecurityRole(SecurityRole securityRole) => this.GetSecurityRole(securityRole.Name);

	public FullSecurityRole GetSecurityRole(string name)
	{
		return this.nameDict.GetValueOrDefault(name) ?? throw new Exception($"SecurityRole with name '{name}' not found");
	}

	public FullSecurityRole GetSecurityRole(SecurityIdentity identity)
	{
		return this.identityDict.GetValueOrDefault(identity) ?? throw new Exception($"SecurityRole with {nameof(identity)} '{identity}' not found");
	}

	public IEnumerable<FullSecurityRole> GetRealRoles()
	{
		return this.SecurityRoles.Where(sr => !sr.Information.IsVirtual);
	}

	private void Validate()
	{
		var identityDuplicates = this.SecurityRoles
			.GetDuplicates(
				new EqualityComparerImpl<FullSecurityRole>(
					(sr1, sr2) => sr1.Identity == sr2.Identity,
					sr => sr.Identity.GetHashCode())).ToList();

		if (identityDuplicates.Any())
		{
			throw new Exception($"SecurityRole '{nameof(FullSecurityRole.Identity)}' duplicates: {identityDuplicates.Join(", ", sr => sr.Identity)}");
		}

		var nameDuplicates = this.SecurityRoles
			.GetDuplicates(
				new EqualityComparerImpl<FullSecurityRole>(
					(sr1, sr2) => sr1.Name == sr2.Name,
					sr => sr.Name.GetHashCode())).ToList();

		if (nameDuplicates.Any())
		{
			throw new Exception($"SecurityRole '{nameof(FullSecurityRole.Name)}' duplicates: {nameDuplicates.Join(", ", sr => sr.Name)}");
		}
	}
}