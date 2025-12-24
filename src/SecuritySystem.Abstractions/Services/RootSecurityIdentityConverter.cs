using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Services;

public class RootSecurityIdentityConverter(IServiceProvider serviceProvider, IEnumerable<Type> types) : ISecurityIdentityConverter
{
    private readonly IReadOnlyList<ISecurityIdentityConverter> converters = types.Distinct().Select(identType =>
        (ISecurityIdentityConverter)serviceProvider.GetRequiredService(typeof(ISecurityIdentityConverter<>).MakeGenericType(identType))).ToList();

    public TypedSecurityIdentity? TryConvert(SecurityIdentity securityIdentity)
    {
        var convertRequest =

            from converter in converters

            let tryConvertedIdentity = converter.TryConvert(securityIdentity)

            where tryConvertedIdentity != null

            select tryConvertedIdentity;

        return convertRequest.Distinct().SingleOrDefault();
    }

    public TypedSecurityIdentity Convert(SecurityIdentity securityIdentity)
    {
        return this.TryConvert(securityIdentity) ?? throw new ArgumentOutOfRangeException(nameof(securityIdentity));
    }
}