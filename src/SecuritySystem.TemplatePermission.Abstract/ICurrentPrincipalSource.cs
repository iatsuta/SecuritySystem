namespace SecuritySystem.TemplatePermission;

public interface ICurrentPrincipalSource<out TPrincipal>
{
    TPrincipal CurrentPrincipal { get; }
}
