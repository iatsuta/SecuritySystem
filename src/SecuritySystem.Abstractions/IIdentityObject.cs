namespace SecuritySystem;

public interface IIdentityObject<out TIdent>
{
    TIdent Id { get; }
}