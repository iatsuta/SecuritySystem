using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.DictionaryCache;

namespace SecuritySystem.Services;

public class IdentityInfoSource(IIdentityPropertyExtractor propertyExtractor, IEnumerable<IdentityInfo> customInfoList) : IIdentityInfoSource
{
    private readonly IDictionaryCache<Type, IdentityInfo> identityInfoCache = new DictionaryCache<Type, IdentityInfo>(domainType =>
    {
        var customInfo = customInfoList.SingleOrDefault(identityInfo => identityInfo.DomainObjectType == domainType);

        if (customInfo != null)
        {
            return customInfo;
        }
        else
        {
            var idProperty = propertyExtractor.Extract(domainType);

            var idPath = idProperty.ToLambdaExpression();

            return new Func<Expression<Func<object, object>>, IdentityInfo<object, object>>(CreateIdentityInfo)
                .CreateGenericMethod(domainType, idProperty.PropertyType)
                .Invoke<IdentityInfo>(null, idPath);
        }

    }).WithLock();

    public IdentityInfo GetIdentityInfo(Type domainObjectType)
    {
        return this.identityInfoCache[domainObjectType];
    }

    private static IdentityInfo<TDomainObject, TIdent> CreateIdentityInfo<TDomainObject, TIdent>(Expression<Func<TDomainObject, TIdent>> idPath)
        where TIdent : notnull
    {
        return new IdentityInfo<TDomainObject, TIdent>(idPath);
    }
}