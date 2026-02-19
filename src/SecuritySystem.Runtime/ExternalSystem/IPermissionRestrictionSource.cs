using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.ExternalSystem;

public interface IPermissionRestrictionSource<TPermission>
{
    Expression<Func<TPermission, bool>> GetUnrestrictedFilter();
}

public interface IPermissionRestrictionSource<TPermission, TSecurityContextIdent> : IPermissionRestrictionSource<TPermission>
{
    Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetIdentsExpr();

    Expression<Func<TPermission, bool>> GetContainsIdentsExpr(IEnumerable<TSecurityContextIdent> idents) => this.GetIdentsExpr()
        .Select(restrictionIdents => restrictionIdents.Any(restrictionIdent => idents.Contains(restrictionIdent)));

    Expression<Func<TPermission, bool>> IPermissionRestrictionSource<TPermission>.GetUnrestrictedFilter()
        => this.GetIdentsExpr().Select(v => !v.Any());
}