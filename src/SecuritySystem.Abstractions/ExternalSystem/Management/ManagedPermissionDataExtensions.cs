using System.Collections.Immutable;

namespace SecuritySystem.ExternalSystem.Management;

public static class ManagedPermissionDataExtensions
{
    public static TManagedPermissionData WithExtendedData<TManagedPermissionData>(this TManagedPermissionData managedPermissionData, string key, object value)
        where TManagedPermissionData : ManagedPermissionData
    {
        var newExtendedData = managedPermissionData.ExtendedData.ToDictionary();

        newExtendedData[key] = value;

        return managedPermissionData with { ExtendedData = newExtendedData.ToImmutableDictionary() };
    }
}