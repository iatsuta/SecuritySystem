using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public interface IVirtualBindingInfoSettingsBuilder<TPermission>
    where TPermission : notnull
{
    public IVirtualBindingInfoSettingsBuilder<TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, IEnumerable<TSecurityContext>>> path)
        where TSecurityContext : ISecurityContext;

    public IVirtualBindingInfoSettingsBuilder<TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, TSecurityContext?>> path)
        where TSecurityContext : ISecurityContext;

    public IVirtualBindingInfoSettingsBuilder<TPermission> AddFilter(Expression<Func<TPermission, bool>> filter) => this.AddFilter(_ => filter);

    public IVirtualBindingInfoSettingsBuilder<TPermission> AddFilter(Func<IServiceProvider, Expression<Func<TPermission, bool>>> getFilter);
}