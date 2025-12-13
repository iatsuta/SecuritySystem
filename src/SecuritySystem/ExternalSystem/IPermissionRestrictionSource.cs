using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.ExternalSystem;

public interface IPermissionRestrictionSource<TPermission, TSecurityContextIdent>
{
    Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetIdentsExpr();

    Expression<Func<TPermission, bool>> GetGrandAccessExpr() => this.GetIdentsExpr().Select(v => !v.Any());

    Expression<Func<TPermission, bool>> GetContainsIdentsExpr(IEnumerable<TSecurityContextIdent> idents) => this.GetIdentsExpr()
        .Select(restrictionIdents => restrictionIdents.Any(restrictionIdent => idents.Contains(restrictionIdent)));
}