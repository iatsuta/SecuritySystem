namespace SecuritySystem.VirtualPermission.DependencyInjection;

public interface IVirtualBindingInfoRootSettingsBuilder<TPermission>
    where TPermission : notnull
{
    IVirtualBindingInfoRootSettingsBuilder<TPermission> ForRole(SecurityRole securityRole, Action<IVirtualBindingInfoSettingsBuilder<TPermission>>? init = null);
}