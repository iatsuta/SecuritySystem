namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public record SecurityContextData<TSecurityContextIdent>(TSecurityContextIdent Id, string Name, TSecurityContextIdent? ParentId)
    where TSecurityContextIdent : notnull
{
    public SecurityContextData<object> UpCast() => new(this.Id, this.Name, this.ParentId);
}