using CommonFramework;

using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission.Validation;

public class PermissionDataComparer<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
	GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>
		bindingInfo)
	: IEqualityComparer<PermissionData<TPermission, TPermissionRestriction>>
	where TPrincipal : class
	where TPermission : class
	where TSecurityRole : class
	where TPermissionRestriction : class
	where TSecurityContextType : class
	where TSecurityContextObjectIdent : notnull
{
	protected virtual IEqualityComparer<IGrouping<TSecurityContextType, TSecurityContextObjectIdent>> RestrictionGroupComparer { get; } =

		new EqualityComparerImpl<IGrouping<TSecurityContextType, TSecurityContextObjectIdent>>(
			(g1, g2) => g1.Key == g2.Key && g1.SequenceEqual(g2),
			g => g.Key.GetHashCode());

	public bool Equals(PermissionData<TPermission, TPermissionRestriction>? permissionData,
		PermissionData<TPermission, TPermissionRestriction>? otherPermissionData)
	{
		if (object.ReferenceEquals(permissionData, otherPermissionData))
		{
			return true;
		}

		if (permissionData is null || otherPermissionData is null)
		{
			return false;
		}

		return this.PureEquals(permissionData, otherPermissionData);
	}

	protected virtual bool PureEquals(
		PermissionData<TPermission, TPermissionRestriction> permissionData,
		PermissionData<TPermission, TPermissionRestriction> otherPermissionData)
	{
		return bindingInfo.SecurityRole.Getter(permissionData.Permission) == bindingInfo.SecurityRole.Getter(otherPermissionData.Permission)
		       && (bindingInfo.PermissionPeriod == null
		           || this.IsIntersected(bindingInfo.PermissionPeriod.Getter(permissionData.Permission), bindingInfo.PermissionPeriod.Getter(otherPermissionData.Permission)))
		       && this.EqualsRestrictions(permissionData, otherPermissionData);
	}

	protected virtual bool IsIntersected((DateTime StartDate, DateTime? EndDate) period, (DateTime StartDate, DateTime? EndDate) otherPeriod)
	{
		var periodEnd = period.EndDate ?? DateTime.MaxValue;
		var otherPeriodEnd = otherPeriod.EndDate ?? DateTime.MaxValue;

		return period.StartDate <= otherPeriodEnd && otherPeriod.StartDate <= periodEnd;
	}

	protected virtual bool EqualsRestrictions(
		PermissionData<TPermission, TPermissionRestriction> permissionData,
		PermissionData<TPermission, TPermissionRestriction> otherPermissionData)
	{
		return this.GetOrderedIdents(permissionData).SequenceEqual(this.GetOrderedIdents(otherPermissionData), this.RestrictionGroupComparer);
	}

	protected IEnumerable<IGrouping<TSecurityContextType, TSecurityContextObjectIdent>> GetOrderedIdents(
		PermissionData<TPermission, TPermissionRestriction> permissionData)
	{
		return

			from permissionRestriction in permissionData.Restrictions

			orderby bindingInfo.SecurityContextObjectId

			group bindingInfo.SecurityContextObjectId.Getter(permissionRestriction) by bindingInfo.SecurityContextType.Getter(permissionRestriction)

			into g

			orderby g.Key

			select g;
	}

	public int GetHashCode(PermissionData<TPermission, TPermissionRestriction> permissionData)
	{
		return permissionData.Restrictions.Count ^ bindingInfo.SecurityRole.Getter(permissionData.Permission).GetHashCode();
	}
}