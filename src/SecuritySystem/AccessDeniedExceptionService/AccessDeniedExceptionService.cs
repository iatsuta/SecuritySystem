﻿using CommonFramework;

using SecuritySystem.Providers;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class AccessDeniedExceptionService : IAccessDeniedExceptionService
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
                                       var messagePrefix = (domainObject as IIdentityObject<Guid>).Maybe(v => v.Id == Guid.Empty)
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

    protected virtual IEnumerable<KeyValuePair<string, object>> GetAccessDeniedExceptionMessageElements(object domainObject, Type domainObjectType, SecurityRule? securityRule)
    {
        //if (domainObject is IVisualIdentityObject visualIdentityObject)
        //{
        //    var name = visualIdentityObject.Name;

        //    yield return new KeyValuePair<string, object>("name", name);
        //}

        yield return new KeyValuePair<string, object>("type", domainObjectType.Name);

        if (domainObject is IIdentityObject<Guid> identityObject && identityObject.Id != Guid.Empty)
        {
            yield return new KeyValuePair<string, object>("id", identityObject.Id);
        }

        if (securityRule != null)
        {
            yield return new KeyValuePair<string, object>("securityRule", securityRule);
        }
    }
}
