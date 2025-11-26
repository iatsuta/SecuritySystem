using CommonFramework;





using SecuritySystem.Attributes;
using SecuritySystem.Services;

namespace SecuritySystem.TemplatePermission;

public class CurrentPrincipalSource(
    [DisabledSecurity] IRepository<TPrincipal> principalRepository,
    IRawUserAuthenticationService userAuthenticationService) : ICurrentPrincipalSource
{
    private readonly Lazy<TPrincipal> currentPrincipalLazy = LazyHelper.Create(
        () =>
        {
            var userName = userAuthenticationService.GetUserName();

            return principalRepository
                   .GetQueryable().SingleOrDefault(principal => principal.Name == userName)
                   ?? new TPrincipal { Name = userName };
        });

    public TPrincipal CurrentPrincipal => this.currentPrincipalLazy.Value;
}
