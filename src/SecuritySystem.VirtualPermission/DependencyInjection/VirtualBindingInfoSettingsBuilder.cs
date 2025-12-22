using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public class VirtualBindingInfoSettingsBuilder<TPermission> : IVirtualBindingInfoSettingsBuilder<TPermission>
    where TPermission : notnull
{
    private readonly List<Func<VirtualPermissionBindingInfo<TPermission>, VirtualPermissionBindingInfo<TPermission>>> initList = new();

    public IVirtualBindingInfoSettingsBuilder<TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, IEnumerable<TSecurityContext>>> path)
        where TSecurityContext : ISecurityContext
    {
        this.initList.Add(v => v with { Restrictions = v.Restrictions.Concat([path]).ToList() });

        return this;
    }

    public IVirtualBindingInfoSettingsBuilder<TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, TSecurityContext?>> path)
        where TSecurityContext : ISecurityContext
    {
        this.initList.Add(v => v with { Restrictions = v.Restrictions.Concat([path]).ToList() });

        return this;
    }

    public IVirtualBindingInfoSettingsBuilder<TPermission> AddFilter(
        Func<IServiceProvider, Expression<Func<TPermission, bool>>> getFilter)
    {
        this.initList.Add(v => v with { GetFilter = sp => v.GetFilter(sp).BuildAnd(getFilter(sp)) });

        return this;
    }

    public VirtualPermissionBindingInfo<TPermission> Init(VirtualPermissionBindingInfo<TPermission> virtualBindingInfo)
    {
        return this.initList.Aggregate(virtualBindingInfo, (v, f) => f(v));
    }
}