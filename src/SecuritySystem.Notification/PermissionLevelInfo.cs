namespace SecuritySystem.Notification;

public record PermissionLevelInfo<TPermission>
{
    public required TPermission Permission { get; init; }

    public required string LevelInfo { get; init; }
}

public record FullPermissionLevelInfo<TPermission> : PermissionLevelInfo<TPermission>
{
    public required int Level { get; init; }
}

public record PermissionLevelDictInfo<TPermission>
{
    public required TPermission Permission { get; init; }

    public required Dictionary<Type, int> LevelDict { get; init; }
}