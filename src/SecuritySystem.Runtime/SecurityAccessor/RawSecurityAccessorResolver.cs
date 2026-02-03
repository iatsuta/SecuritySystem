namespace SecuritySystem.SecurityAccessor;

public class RawSecurityAccessorResolver(ISecurityAccessorInfinityStorage infinityStorage) : ISecurityAccessorResolver
{
    public const string Key = "Raw";

    public virtual IEnumerable<string> Resolve(SecurityAccessorData securityAccessorData)
    {
        return securityAccessorData switch
        {
            SecurityAccessorData.FixedSecurityAccessorData fixedResult => fixedResult.Items,

            SecurityAccessorData.AndSecurityAccessorData { Right: SecurityAccessorData.NegateSecurityAccessorData right } andNegateResult =>
                this.Resolve(andNegateResult.Left).Except(this.Resolve(right.InnerData), StringComparer.CurrentCultureIgnoreCase),

            SecurityAccessorData.AndSecurityAccessorData andResult => this.Resolve(andResult.Left)
                .Intersect(this.Resolve(andResult.Right), StringComparer.CurrentCultureIgnoreCase),

            SecurityAccessorData.OrSecurityAccessorData orResult => this.Resolve(orResult.Left)
                .Union(this.Resolve(orResult.Right), StringComparer.CurrentCultureIgnoreCase),

            SecurityAccessorData.InfinitySecurityAccessorData => infinityStorage.GetInfinityData(),

            SecurityAccessorData.NegateSecurityAccessorData negateResult => infinityStorage.GetInfinityData().Except(this.Resolve(negateResult.InnerData)),

            _ => throw new ArgumentOutOfRangeException(nameof(securityAccessorData))
        };
    }
}
