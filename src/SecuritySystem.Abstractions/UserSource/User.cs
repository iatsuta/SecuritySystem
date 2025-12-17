namespace SecuritySystem.UserSource;

public record User(string Name, TypedSecurityIdentity Identity);