using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public interface IVirtualBindingInfoSettingsBuilder<TPermission>
    where TPermission : notnull
{
    IVirtualBindingInfoSettingsBuilder<TPermission> AddFilter(Expression<Func<TPermission, bool>> filter) => this.AddFilter(_ => filter);

    IVirtualBindingInfoSettingsBuilder<TPermission> AddFilter(Func<IServiceProvider, Expression<Func<TPermission, bool>>> getFilter);
}