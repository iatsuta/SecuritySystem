// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public interface ISecurityPathRestrictionService
{
    SecurityPath<TDomainObject> ApplyRestriction<TDomainObject>(
        SecurityPath<TDomainObject> securityPath,
        SecurityPathRestriction restriction);
}
