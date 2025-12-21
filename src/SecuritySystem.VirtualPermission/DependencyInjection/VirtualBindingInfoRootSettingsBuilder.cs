namespace SecuritySystem.VirtualPermission.DependencyInjection;

public class VirtualBindingInfoRootSettingsBuilder<TPermission> : IVirtualBindingInfoRootSettingsBuilder<TPermission>
    where TPermission : notnull
{
    public readonly List<VirtualPermissionBindingInfo<TPermission>> VirtualPermissionBindingInfoList = new();

    public IVirtualBindingInfoRootSettingsBuilder<TPermission> ForRole(SecurityRole securityRole,
        Action<IVirtualBindingInfoSettingsBuilder<TPermission>>? init = null)
    {
        var innerBuilder = new VirtualBindingInfoSettingsBuilder<TPermission>();

        init?.Invoke(innerBuilder);

        var virtualBindingInfo = innerBuilder.Init(new VirtualPermissionBindingInfo<TPermission> { SecurityRole = securityRole });

        this.VirtualPermissionBindingInfoList.Add(virtualBindingInfo);

        return this;
    }
}