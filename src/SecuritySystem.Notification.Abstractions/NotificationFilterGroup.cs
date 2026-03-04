using System.Collections.Immutable;

namespace SecuritySystem.Notification;

public abstract record NotificationFilterGroup
{
    public required Type SecurityContextType { get; init; }

    public required NotificationExpandType ExpandType { get; init; }
};

public record NotificationFilterGroup<TIdent>(ImmutableArray<TIdent> Idents) : NotificationFilterGroup;