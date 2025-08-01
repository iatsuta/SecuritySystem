using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.DictionaryCache;

namespace SecuritySystem.Services;

public class IdentityInfoSource(IServiceProvider serviceProvider) : IIdentityInfoSource
{
    private readonly IDictionaryCache<Type, IdentityInfo?> identityInfoCache = new DictionaryCache<Type, IdentityInfo?>(domainObjectType =>
    {
        var baseIdentityInfo = (IdentityInfo?)serviceProvider.GetService(typeof(IdentityInfo<>).MakeGenericType(domainObjectType));

        if (baseIdentityInfo != null)
        {
            return baseIdentityInfo;
        }
        
        var idProperty = domainObjectType.GetProperty("Id");

        if (idProperty != null)
        {
            var idPath = idProperty.ToLambdaExpression();

            return new Func<Expression<Func<object, object>>, IdentityInfo<object, object>>(CreateIdentityInfo)
                .CreateGenericMethod(domainObjectType, idProperty.PropertyType)
                .Invoke<IdentityInfo>(null, idPath);
        }
        else
        {
            return null;
        }

    }).WithLock();

    public IdentityInfo? TryGetIdentityInfo(Type domainObjectType)
    {
        return this.identityInfoCache[domainObjectType];
    }

    private static IdentityInfo<TDomainObject, TIdent> CreateIdentityInfo<TDomainObject, TIdent>(Expression<Func<TDomainObject, TIdent>> idPath)
        where TIdent : notnull
    {
        return new IdentityInfo<TDomainObject, TIdent>(idPath);
    }
}