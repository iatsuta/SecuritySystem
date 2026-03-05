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

    public IAsyncEnumerable<TPermission> GetPermissionsAsync(
        IEnumerable<SecurityRole> securityRoles,
        IEnumerable<NotificationFilterGroup> notificationFilterGroups)
    {
        var cachedNotificationFilterGroups = notificationFilterGroups.ToArray();

        var startPermissionQ = queryableSource.GetQueryable<TPermission>()
            .Where(notificationGeneralPermissionFilterFactory.Create(securityRoles))
            .Select(p => new PermissionLevelInfo<TPermission> { Permission = p, LevelInfo = "" });

        var permissionInfoResult = cachedNotificationFilterGroups.Aggregate(startPermissionQ, this.ApplyNotificationFilter).GenericAsAsyncEnumerable();

        var typeDict = cachedNotificationFilterGroups.Select(g => g.SecurityContextType).ToDictionary(g => g.Name);

        var parsedLevelInfoResult =
            permissionInfoResult
                .Select(principalInfo => new PermissionLevelDictInfo<TPermission>
                {
                    Permission = principalInfo.Permission,
                    LevelDict = principalInfo.LevelInfo
                        .Split(LevelsSeparator, StringSplitOptions.RemoveEmptyEntries)
                        .Select(levelData => levelData.Split(LevelValueSeparator))
                        .ToDictionary(
                            levelParts => typeDict[levelParts[0]],
                            levelParts => int.Parse(levelParts[1]))
                });

        var optimalRequest = cachedNotificationFilterGroups.Aggregate(parsedLevelInfoResult, (state, g) => new ApplyPermissionLevelDictInfoAsyncEnumerable(state, g));

        return optimalRequest.Select(pair => pair.Permission).Distinct();
    }

    private class ApplyPermissionLevelDictInfoAsyncEnumerable(
        IAsyncEnumerable<PermissionLevelDictInfo<TPermission>> state,
        NotificationFilterGroup notificationFilterGroup) : IAsyncEnumerable<PermissionLevelDictInfo<TPermission>>
    {
        public async IAsyncEnumerator<PermissionLevelDictInfo<TPermission>> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            await using var enumerator = state.GetAsyncEnumerator(cancellationToken);

            if (!await enumerator.MoveNextAsync())
            {
                yield break;
            }

            var reloadedEnumerable = AsyncEnumerable.Repeat(enumerator.Current, 1).Concat(enumerator.ReadToEnd());

            if (notificationFilterGroup.ExpandType == NotificationExpandType.All)
            {
                await foreach (var value in reloadedEnumerable)
                {
                    yield return value;
                }
            }
            else
            {
                var request =

                    from pair in reloadedEnumerable

                    group pair by pair.LevelDict[notificationFilterGroup.SecurityContextType]

                    into levelGroup

                    orderby levelGroup.Key descending

                    select levelGroup;

                foreach (var permissionLevelDictInfo in await request.FirstAsync(cancellationToken))
                {
                    yield return permissionLevelDictInfo;
                }
            }
        }
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

internal static class AsyncEnumeratorExtensions
{
    public static async IAsyncEnumerable<T> ReadToEnd<T>(this IAsyncEnumerator<T> enumerator)
    {
        while (await enumerator.MoveNextAsync())
        {
            yield return enumerator.Current;
        }
    }
}