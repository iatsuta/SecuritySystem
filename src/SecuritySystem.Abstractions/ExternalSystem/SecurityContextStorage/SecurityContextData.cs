namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public record SecurityContextData<TIdent>(TIdent Id, string Name, TIdent? ParentId)
    where TIdent : notnull
{
    public SecurityContextData<object> UpCast() => new(this.Id, this.Name, this.ParentId);
}