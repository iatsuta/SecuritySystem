namespace SecuritySystem.SecurityAccessor;

public class FakeSecurityAccessorInfinityStorage : ISecurityAccessorInfinityStorage
{
    public IEnumerable<string> GetInfinityData()
    {
        throw new InvalidOperationException("Use 'SetSecurityAccessorInfinityStorage' for initialize infinity storage");
    }
}