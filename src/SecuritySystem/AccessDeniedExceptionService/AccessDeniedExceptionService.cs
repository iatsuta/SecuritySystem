using CommonFramework;

using SecuritySystem.Providers;
using SecuritySystem.Services;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class AccessDeniedExceptionService(IIdentityInfoSource identityInfoSource) : IAccessDeniedExceptionService
{
    public Exception GetAccessDeniedException(AccessResult.AccessDeniedResult accessDeniedResult)
    {
        return new AccessDeniedException(this.GetAccessDeniedExceptionMessage(accessDeniedResult));
    }

    protected virtual string GetAccessDeniedExceptionMessage(AccessResult.AccessDeniedResult accessDeniedResult)
    {
        if (accessDeniedResult.CustomMessage != null)
        {
            return accessDeniedResult.CustomMessage;
        }
        else
        {
            var securityRule = accessDeniedResult.SecurityRule;

            if (accessDeniedResult.DomainObjectInfo == null)
            {
                if (securityRule == null)
                {
                    return $"You are not authorized to perform operation";
                }
                else
                {
                    return $"You are not authorized to perform '{securityRule}' operation";
                }
            }
            else
            {
                var info = accessDeniedResult.DomainObjectInfo.Value;

                return this.GetAccessDeniedExceptionMessage(info.DomainObject, info.DomainObjectType, securityRule);
            }
        }
    }

    protected virtual string GetAccessDeniedExceptionMessage(object domainObject, Type domainObjectType, SecurityRule? securityRule)
    {
        var elements = this.GetAccessDeniedExceptionMessageElements(domainObject, domainObjectType, securityRule).ToDictionary();

        return elements.GetByFirst((first, other) =>
        {
            var messagePrefix = this.TryGetId(domainObject, domainObjectType) == null
                ? "You have no permissions to create object"
                : "You have no permissions to access object";

            var messageBody = $" with {this.PrintElement(first.Key, first.Value)}";

            var messagePostfix = other.Any() ? $" ({other.Join(", ", pair => this.PrintElement(pair.Key, pair.Value))})" : "";

            return messagePrefix + messageBody + messagePostfix;
        });
    }

    protected virtual string PrintElement(string key, object value)
    {
        return $"{key} = '{value}'";
    }

    protected virtual IEnumerable<KeyValuePair<string, object>> GetAccessDeniedExceptionMessageElements(object domainObject, Type domainObjectType,
        SecurityRule? securityRule)
    {
        yield return new KeyValuePair<string, object>("type", domainObjectType.Name);

        var id = this.TryGetId(domainObject, domainObjectType);

        if (id != null)
        {
            yield return new KeyValuePair<string, object>("id", id);
        }

        if (securityRule != null)
        {
            yield return new KeyValuePair<string, object>("securityRule", securityRule);
        }
    }

    private object? TryGetId(object domainObject, Type domainObjectType)
    {
        var identityInfo = identityInfoSource.GetIdentityInfo(domainObjectType);

        return new Func<object, IdentityInfo<object, object>, object?>(this.TryGetId).CreateGenericMethod(domainObjectType, identityInfo.IdentityType)
            .Invoke<object?>(this, domainObject, identityInfo);
    }

    private object? TryGetId<TDomainObject, TIdent>(TDomainObject domainObject, IdentityInfo<TDomainObject, TIdent> identityInfo)
        where TIdent : notnull
    {
        var comparer = EqualityComparer<TIdent>.Default;

        var id = identityInfo.Id.Getter(domainObject);

        if (comparer.Equals(id, default))
        {
            return null;
        }
        else
        {
            return id;
        }
    }
}