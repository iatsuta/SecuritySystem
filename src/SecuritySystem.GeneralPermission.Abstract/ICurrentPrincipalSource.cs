namespace SecuritySystem.GeneralPermission;

public interface ICurrentPrincipalSource<out TPrincipal>
{
    TPrincipal CurrentPrincipal { get; }
}
