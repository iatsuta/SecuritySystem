namespace SecuritySystem.Services;

public interface IVisualIdentityInfoSource
{
	VisualIdentityInfo<TDomainObject> GetVisualIdentityInfo<TDomainObject>();

	VisualIdentityInfo<TDomainObject>? TryGetVisualIdentityInfo<TDomainObject>();
}