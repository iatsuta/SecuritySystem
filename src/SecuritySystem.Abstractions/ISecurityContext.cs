﻿namespace SecuritySystem;

/// <summary>
/// Интерфейс доменного типа авторизации для типизированного контекста.
/// </summary>
public interface ISecurityContext : IIdentityObject<Guid>;