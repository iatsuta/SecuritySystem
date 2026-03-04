using CommonFramework.GenericRepository;

using GenericQueryable;

namespace SecuritySystem.Notification;

public class NotificationPermissionExtractor<TPermission>(
    INotificationGeneralPermissionFilterFactory<TPermission> notificationGeneralPermissionFilterFactory,
    IPermissionLevelInfoExtractor<TPermission> permissionLevelInfoExtractor,
    IQueryableSource queryableSource)
    : INotificationPermissionExtractor<TPermission>
    where TPermission : class
{
    private const string LevelsSeparator = "|";

    private const string LevelValueSeparator = ":";

    public async Task<List<TPermission>> GetPermissionsAsync(
        SecurityRole[] securityRoles,
        IEnumerable<NotificationFilterGroup> notificationFilterGroups,
        CancellationToken cancellationToken)
    {
        var cachedNotificationFilterGroups = notificationFilterGroups.ToArray();

        var startPermissionQ = queryableSource.GetQueryable<TPermission>()
                                                   .Where(notificationGeneralPermissionFilterFactory.Create(securityRoles))
                                                   .Select(p => new PermissionLevelInfo<TPermission> { Permission = p, LevelInfo = "" });

        var permissionInfoResult = await cachedNotificationFilterGroups.Aggregate(startPermissionQ, this.ApplyNotificationFilter).GenericToListAsync(cancellationToken);

        var typeDict = cachedNotificationFilterGroups.Select(g => g.SecurityContextType).ToDictionary(g => g.Name);

        var parsedLevelInfoResult =
            permissionInfoResult
                .Select(principalInfo => new
                                         {
                                             principalInfo.Permission,
                                             LevelDict = principalInfo.LevelInfo
                                                                      .Split(LevelsSeparator, StringSplitOptions.RemoveEmptyEntries)
                                                                      .Select(levelData => levelData.Split(LevelValueSeparator))
                                                                      .ToDictionary(
                                                                          levelParts => typeDict[levelParts[0]],
                                                                          levelParts => int.Parse(levelParts[1]))
                                         })
                .ToList();

        var optimalRequest = cachedNotificationFilterGroups.Aggregate(
            parsedLevelInfoResult,
            (state, notificationFilterGroup) =>
            {
                if (notificationFilterGroup.ExpandType == NotificationExpandType.All || !state.Any())
                {
                    return state;
                }
                else
                {
                    var request = from pair in state

                                  group pair by pair.LevelDict[notificationFilterGroup.SecurityContextType]

                                  into levelGroup

                                  orderby levelGroup.Key descending

                                  select levelGroup;

                    return request.First().ToList();
                }
            });

        return await optimalRequest.Select(pair => pair.Permission).Distinct().ToAsyncEnumerable().ToListAsync(cancellationToken);
    }

    private IQueryable<PermissionLevelInfo<TPermission>> ApplyNotificationFilter(
        IQueryable<PermissionLevelInfo<TPermission>> source,
        NotificationFilterGroup notificationFilterGroup)
    {
        var selector = permissionLevelInfoExtractor.GetSelector(notificationFilterGroup);

        return from permissionLevelInfo in source.Select(selector)

               where permissionLevelInfo.Level != PriorityLevels.AccessDenied

               select new PermissionLevelInfo<TPermission>
                      {
                          Permission = permissionLevelInfo.Permission,
                          LevelInfo = permissionLevelInfo.LevelInfo
                                      + $"{LevelsSeparator}{notificationFilterGroup.SecurityContextType.Name}{LevelValueSeparator}{permissionLevelInfo.Level}"
                      };
    }
}
