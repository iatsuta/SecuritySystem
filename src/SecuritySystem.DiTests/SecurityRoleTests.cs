using HierarchicalExpand;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.DiTests.Rules;
using SecuritySystem.Expanders;

namespace SecuritySystem.DiTests;

public class SecurityRoleTests : TestBase
{
    [Fact]
    public void AdministratorRole_ShouldNotContains_SystemIntegrationRole()
    {
        // Arrange
        var securityRoleSource = this.RootServiceProvider.GetRequiredService<ISecurityRoleSource>();

        // Act
        var adminRole = securityRoleSource.GetSecurityRole(SecurityRole.Administrator);

        // Assert
        adminRole.Information.Children.Contains(SecurityRole.SystemIntegration).Should().BeFalse();
    }

    [Fact]
    public void SecurityRoleExpander_ExpandDeepChild_AllRolesExpanded()
    {
        // Arrange
        var expander = this.RootServiceProvider.GetRequiredService<ISecurityRoleGroupExpander>();

        var expectedResult = new DomainSecurityRule.ExpandedRoleGroupSecurityRule(
        [
            new DomainSecurityRule.ExpandedRolesSecurityRule([ExampleSecurityRole.TestRole, ExampleSecurityRole.TestRole2, ExampleSecurityRole.TestRole3])
                { CustomRestriction = SecurityPathRestriction.Default },

            new DomainSecurityRule.ExpandedRolesSecurityRule([SecurityRole.Administrator]) { CustomRestriction = SecurityPathRestriction.Ignored },
        ]);

        // Act
        var expandResult = expander.Expand(ExampleSecurityRole.TestRole3);

        // Assert
        expandResult.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void SecurityRoleExpander_ExpandWithDefaultExpandType_RoleResolved()
    {
        // Arrange
        var expander = this.RootServiceProvider.GetRequiredService<ISecurityOperationExpander>();

        // Act
        var expandResult = expander.Expand(new DomainSecurityRule.OperationSecurityRule(ExampleSecurityOperation.EmployeeView));

        // Assert
        expandResult.SecurityRoles.Should().BeEquivalentTo([ExampleSecurityRole.TestRole]);
    }

    [Fact]
    public void SecurityRoleExpander_ExpandWithCustomExpandType_SecurityRuleCorrected()
    {
        // Arrange
        var expander = this.RootServiceProvider.GetRequiredService<ISecurityOperationExpander>();

        // Act
        var expandResult = expander.Expand(ExampleSecurityOperation.EmployeeView.ToSecurityRule(HierarchicalExpandType.None));

        // Assert
        expandResult.Should().BeEquivalentTo(new[] { ExampleSecurityRole.TestRole }.ToSecurityRule(HierarchicalExpandType.None));
    }

    [Fact]
    public void SecurityRoleExpander_FullExpandWithCustomExpandType_SecurityRuleCorrected()
    {
        // Arrange
        var expander = this.RootServiceProvider.GetRequiredService<ISecurityRuleExpander>();

        var customExpandType = HierarchicalExpandType.All;

        var expectedResult = new DomainSecurityRule.ExpandedRoleGroupSecurityRule(
        [
            new DomainSecurityRule.ExpandedRolesSecurityRule([ExampleSecurityRole.TestRole])
                { CustomRestriction = SecurityPathRestriction.Default, CustomExpandType = customExpandType },

            new DomainSecurityRule.ExpandedRolesSecurityRule([SecurityRole.Administrator])
                { CustomRestriction = SecurityPathRestriction.Ignored }
        ]);

        // Act
        var expandResult = expander.FullRoleExpand(ExampleSecurityOperation.EmployeeView.ToSecurityRule(customExpandType));

        // Assert
        expandResult.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void SecurityRoleExpander_FullExpandWithCustomExpandTypeFromOperations_SecurityRuleCorrected()
    {
        // Arrange
        var expander = this.RootServiceProvider.GetRequiredService<ISecurityRuleExpander>();

        var customExpandType = HierarchicalExpandType.None;

        // Act
        var expandResult = expander.FullRoleExpand(ExampleSecurityOperation.BusinessUnitView);

        var expectedResult = new DomainSecurityRule.ExpandedRoleGroupSecurityRule(
        [
            new DomainSecurityRule.ExpandedRolesSecurityRule([ExampleSecurityRole.TestRole4])
                { CustomRestriction = SecurityPathRestriction.Default, CustomExpandType = customExpandType },

            new DomainSecurityRule.ExpandedRolesSecurityRule([SecurityRole.Administrator])
                { CustomRestriction = SecurityPathRestriction.Ignored }
        ]);

        // Assert
        expandResult.Should().BeEquivalentTo(expectedResult);
    }
}