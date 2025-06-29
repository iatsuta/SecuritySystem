﻿namespace SecuritySystem.UserSource;

public interface ICurrentUserSource<out TUser>
{
    TUser CurrentUser { get; }
}
