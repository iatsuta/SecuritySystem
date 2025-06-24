namespace SecuritySystem.Services;

public abstract class SecurityRuleVisitor
{
    protected virtual DomainSecurityRule Visit(DomainSecurityRule.ExpandedRolesSecurityRule securityRule)
    {
        return securityRule;
    }

    protected virtual DomainSecurityRule Visit(DomainSecurityRule.NonExpandedRolesSecurityRule securityRule)
    {
        return securityRule;
    }

    protected virtual DomainSecurityRule Visit(DomainSecurityRule.DomainModeSecurityRule securityRule)
    {
        return securityRule;
    }

    protected virtual DomainSecurityRule Visit(DomainSecurityRule.ClientSecurityRule securityRule)
    {
        return securityRule;
    }

    protected virtual DomainSecurityRule Visit(DomainSecurityRule.SecurityRuleHeader securityRule)
    {
        return securityRule;
    }

    protected virtual DomainSecurityRule Visit(DomainSecurityRule.OperationSecurityRule securityRule)
    {
        return securityRule;
    }

    protected virtual DomainSecurityRule Visit(DomainSecurityRule.OrSecurityRule baseSecurityRule)
    {
        var visitedLeft = this.Visit(baseSecurityRule.Left);

        var visitedRight = this.Visit(baseSecurityRule.Right);

        if (baseSecurityRule.Left == visitedLeft && baseSecurityRule.Right == visitedRight)
        {
            return baseSecurityRule;
        }
        else
        {
            return visitedLeft.Or(visitedRight);
        }
    }

    protected virtual DomainSecurityRule Visit(DomainSecurityRule.AndSecurityRule baseSecurityRule)
    {
        var visitedLeft = this.Visit(baseSecurityRule.Left);

        var visitedRight = this.Visit(baseSecurityRule.Right);

        if (baseSecurityRule.Left == visitedLeft && baseSecurityRule.Right == visitedRight)
        {
            return baseSecurityRule;
        }
        else
        {
            return visitedLeft.And(visitedRight);
        }
    }

    protected virtual DomainSecurityRule Visit(DomainSecurityRule.NegateSecurityRule baseSecurityRule)
    {
        var visitedInner = this.Visit(baseSecurityRule.InnerRule);

        if (baseSecurityRule.InnerRule == visitedInner)
        {
            return baseSecurityRule;
        }
        else
        {
            return visitedInner.Negate();
        }
    }

    protected virtual DomainSecurityRule Visit(DomainSecurityRule.RoleBaseSecurityRule baseSecurityRule) => baseSecurityRule switch
    {
        DomainSecurityRule.ExpandedRolesSecurityRule securityRule => this.Visit(securityRule),
        DomainSecurityRule.NonExpandedRolesSecurityRule securityRule => this.Visit(securityRule),
        DomainSecurityRule.OperationSecurityRule securityRule => this.Visit(securityRule),
        _ => baseSecurityRule
    };

    public virtual DomainSecurityRule Visit(DomainSecurityRule baseSecurityRule) => baseSecurityRule switch
    {
        DomainSecurityRule.RoleBaseSecurityRule securityRule => this.Visit(securityRule),
        DomainSecurityRule.DomainModeSecurityRule securityRule => this.Visit(securityRule),
        DomainSecurityRule.ClientSecurityRule securityRule => this.Visit(securityRule),
        DomainSecurityRule.SecurityRuleHeader securityRule => this.Visit(securityRule),
        DomainSecurityRule.OrSecurityRule securityRule => this.Visit(securityRule),
        DomainSecurityRule.AndSecurityRule securityRule => this.Visit(securityRule),
        DomainSecurityRule.NegateSecurityRule securityRule => this.Visit(securityRule),
        _ => baseSecurityRule
    };
}
