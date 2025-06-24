namespace SecuritySystem.SecurityAccessor;

public class FakeSecurityAccessorInfinityStorage : ISecurityAccessorInfinityStorage
{
    public IEnumerable<string> GetInfinityData()
    {
        throw new Exception("Use 'SetSecurityAccessorInfinityStorage' for initialize infinity storage");
    }
}