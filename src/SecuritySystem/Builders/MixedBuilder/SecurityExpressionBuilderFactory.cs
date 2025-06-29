﻿using SecuritySystem.Builders._Factory;
using SecuritySystem.Builders._Filter;



namespace SecuritySystem.Builders.MixedBuilder;

public class SecurityFilterFactory<TDomainObject>(
    ISecurityFilterFactory<TDomainObject> queryFactory,
    ISecurityFilterFactory<TDomainObject> hasAccessFactory)
    : ISecurityFilterFactory<TDomainObject>
{
    public SecurityFilterInfo<TDomainObject> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule, SecurityPath<TDomainObject> securityPath)
    {
        return new SecurityFilterInfo<TDomainObject>(
            queryFactory.CreateFilter(securityRule, securityPath).InjectFunc,
            hasAccessFactory.CreateFilter(securityRule, securityPath).HasAccessFunc);
    }
}
