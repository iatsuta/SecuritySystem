namespace SecuritySystem.Providers;

/// <summary>
/// Провайдер доступа с фиксированным ответом для одного типа
/// </summary>
/// <typeparam name="TDomainObject"></typeparam>
public class ConstSecurityProvider<TDomainObject>(bool hasAccess) : ISecurityProvider<TDomainObject>
{
    public IQueryable<TDomainObject> InjectFilter(IQueryable<TDomainObject> queryable)
    {
        return queryable.Where(_ => hasAccess);
    }

    public bool HasAccess(TDomainObject _) => hasAccess;
}