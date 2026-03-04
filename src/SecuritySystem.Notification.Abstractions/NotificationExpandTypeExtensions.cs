namespace SecuritySystem.Notification;

public static class NotificationExpandTypeExtensions
{
    extension(NotificationExpandType notificationExpandType)
    {
        public bool IsHierarchical() =>
            notificationExpandType switch
            {
                NotificationExpandType.DirectOrFirstParent or NotificationExpandType.DirectOrFirstParentOrEmpty or NotificationExpandType.All => true,
                _ => false
            };

        public bool AllowEmpty() =>
            notificationExpandType switch
            {
                NotificationExpandType.DirectOrEmpty or NotificationExpandType.DirectOrFirstParentOrEmpty or NotificationExpandType.All => true,
                _ => false
            };
    }
}
