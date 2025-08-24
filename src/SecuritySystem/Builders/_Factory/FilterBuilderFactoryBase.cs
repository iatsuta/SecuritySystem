using CommonFramework;

using SecuritySystem.Services;

namespace SecuritySystem.Builders._Factory;

public abstract class FilterBuilderFactoryBase<TDomainObject, TBuilder>(IIdentityInfoSource identityInfoSource)
{
    public virtual TBuilder CreateBuilder(
        SecurityPath<TDomainObject> baseSecurityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions)
    {
        var securityPathType = baseSecurityPath.GetType();

        if (baseSecurityPath is SecurityPath<TDomainObject>.ConditionPath conditionPath)
        {
            return this.CreateBuilder(conditionPath);
        }
        else if (baseSecurityPath is SecurityPath<TDomainObject>.AndSecurityPath andSecurityPath)
        {
            return this.CreateBuilder(andSecurityPath, securityContextRestrictions);
        }
        else if (baseSecurityPath is SecurityPath<TDomainObject>.OrSecurityPath orSecurityPath)
        {
            return this.CreateBuilder(orSecurityPath, securityContextRestrictions);
        }
        else if (securityPathType.IsGenericTypeImplementation(typeof(SecurityPath<>.NestedManySecurityPath<>)))
        {
            return new Func<SecurityPath<TDomainObject>.NestedManySecurityPath<TDomainObject>, IReadOnlyList<SecurityContextRestriction>, TBuilder>(this.CreateBuilder)
                .CreateGenericMethod(securityPathType.GetGenericArguments().Skip(1).ToArray())
                .Invoke<TBuilder>(this, baseSecurityPath, securityContextRestrictions);
        }
        else if (securityPathType.BaseType.Maybe(baseType => baseType.IsGenericTypeImplementation(typeof(SecurityPath<>))))
        {
            var securityContextType = securityPathType.GetGenericArguments().Skip(1).Single();

            var restrictionFilterInfo = securityContextRestrictions.SingleOrDefault(v => v.SecurityContextType == securityContextType);

            var identityInfo = identityInfoSource.GetIdentityInfo(securityContextType);

            return new Func<SecurityPath<TDomainObject>, SecurityContextRestriction<ISecurityContext>?, IdentityInfo<ISecurityContext, Ignore>, TBuilder>(
                    this.CreateSecurityContextBuilder)
                .CreateGenericMethod([securityContextType, identityInfo.IdentityType])
                .Invoke<TBuilder>(this, baseSecurityPath, restrictionFilterInfo, identityInfo);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(baseSecurityPath));
        }
    }

    protected abstract TBuilder CreateBuilder(SecurityPath<TDomainObject>.ConditionPath securityPath);

    protected abstract TBuilder CreateBuilder<TSecurityContext, TIdent>(
        SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
        IdentityInfo<TSecurityContext, TIdent> identityInfo)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull;

    protected abstract TBuilder CreateBuilder<TSecurityContext, TIdent>(
        SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
        SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
        IdentityInfo<TSecurityContext, TIdent> identityInfo)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull;

    protected abstract TBuilder CreateBuilder(
        SecurityPath<TDomainObject>.OrSecurityPath securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions);

    protected abstract TBuilder CreateBuilder(
        SecurityPath<TDomainObject>.AndSecurityPath securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions);

    protected abstract TBuilder CreateBuilder<TNestedObject>(
        SecurityPath<TDomainObject>.NestedManySecurityPath<TNestedObject> securityPath,
        IReadOnlyList<SecurityContextRestriction> securityContextRestrictions);

    private TBuilder CreateSecurityContextBuilder<TSecurityContext, TIdent>(
        SecurityPath<TDomainObject> securityPath,
        SecurityContextRestriction<TSecurityContext>? restrictionFilterInfo,
        IdentityInfo<TSecurityContext, TIdent> identityInfo)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull
    {
        return securityPath switch
        {
            SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> singleSecurityPath => this.CreateBuilder(
                singleSecurityPath,
                restrictionFilterInfo,
                identityInfo),

            SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> manySecurityPath => this.CreateBuilder(
                manySecurityPath,
                restrictionFilterInfo,
                identityInfo),

            _ => throw new ArgumentOutOfRangeException(nameof(securityPath))
        };
    }
}