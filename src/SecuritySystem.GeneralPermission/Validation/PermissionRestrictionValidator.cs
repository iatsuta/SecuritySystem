using CommonFramework;
using CommonFramework.IdentitySource;

using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public class PermissionRestrictionValidator<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TSecurityContextTypeIdent>(
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    IdentityInfo<TSecurityContextType, TSecurityContextObjectIdent> securityContextObjectIdentityInfo) : ISecurityValidator<TPermissionRestriction>
    where TSecurityContextObjectIdent : notnull
{
    //public PermissionRestrictionValidator(
    //    ISecurityContextInfoSource securityContextInfoSource,
    //    ISecurityRoleSource securityRoleSource,
    //    ISecurityContextStorage securityEntitySource,
    //    ISecurityContextSource securityContextSource,
    //    IIdentityInfoSource identityInfoSource)
    //{
    //    this.securityContextInfoSource = securityContextInfoSource;
    //    this.securityContextSource = securityContextSource;
    //    this.identityInfoSource = identityInfoSource;

    //    this.RuleFor(permissionRestriction => permissionRestriction.SecurityContextType)
    //        .Must((permissionRestriction, securityContextType) =>
    //              {
    //                  var securityRole = securityRoleSource.GetSecurityRole(permissionRestriction.TPermission.Role.Id);

    //                  var securityContextInfo = this.GetSecurityContextInfo(securityContextType);

    //                  var allowedSecurityContexts = securityRole.Information.Restriction.SecurityContextTypes;

    //                  return allowedSecurityContexts == null || allowedSecurityContexts.Contains(securityContextInfo.Type);
    //              })
    //        .WithMessage(permissionRestriction => $"Invalid TSecurityContextType: {permissionRestriction.SecurityContextType.Name}.");

    //    this.RuleFor(permissionRestriction => permissionRestriction.SecurityContextId)
    //        .Must((permissionRestriction, securityContextId) =>
    //              {
    //                  var securityContextTypeInfo =
    //                      securityContextInfoSource.GetSecurityContextInfo(permissionRestriction.SecurityContextType.Id);

    //                  var typedSecurityContextStorage =
    //                      (ITypedSecurityContextStorage<TSecurityContextObjectIdent>)securityEntitySource.GetTyped(securityContextTypeInfo.Type);

    //                  return typedSecurityContextStorage.IsExists(securityContextId);
    //              })
    //        .WithMessage(permissionRestriction =>
    //                         $"{permissionRestriction.SecurityContextType.Name} with id '{permissionRestriction.SecurityContextId}' not exists.");

    //    this.RuleFor(permissionRestriction => permissionRestriction.SecurityContextType)
    //        .Must((permissionRestriction, securityContextType) =>
    //              {
    //                  var securityRole = securityRoleSource.GetSecurityRole(permissionRestriction.TPermission.Role.Id);

    //                  var securityContextInfo = this.GetSecurityContextInfo(securityContextType);

    //                  var securityContextRestriction =
    //                      securityRole.Information.Restriction.SecurityContextRestrictions?.SingleOrDefault(r => r.SecurityContextType
    //                          == securityContextInfo.Type);

    //                  var restrictionFilterInfo = securityContextRestriction?.RawFilter;

    //                  return restrictionFilterInfo == null
    //                         || this.IsAllowed(permissionRestriction.SecurityContextId, restrictionFilterInfo);
    //              })
    //        .WithMessage(permissionRestriction => $"SecurityContext: '{permissionRestriction.SecurityContextId}' denied by filter.");
    //}

    public Task ValidateAsync(TPermissionRestriction permissionRestriction, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private SecurityContextInfo GetSecurityContextInfo(TSecurityContextType securityContextType)
    {
        var securityContextTypeId = securityContextObjectIdentityInfo.Id.Getter(securityContextType);

        return securityContextInfoSource.GetSecurityContextInfo(TypedSecurityIdentity.Create(securityContextTypeId));
    }

    private bool IsAllowed(TSecurityContextObjectIdent securityContextId, SecurityContextRestrictionFilterInfo restrictionFilterInfo)
    {
        return new Func<TSecurityContextObjectIdent, SecurityContextRestrictionFilterInfo<ISecurityContext>, bool>(this.IsAllowed)
            .CreateGenericMethod(restrictionFilterInfo.SecurityContextType)
            .Invoke<bool>(this, securityContextId, restrictionFilterInfo);
    }

    private bool IsAllowed<TSecurityContext>(
        TSecurityContextObjectIdent securityContextId,
        SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextObjectIdent>();

        return securityContextSource.GetQueryable(restrictionFilterInfo).Select(identityInfo.Id.Path).Contains(securityContextId);
    }
}