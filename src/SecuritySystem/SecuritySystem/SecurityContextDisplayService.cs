﻿// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class SecurityContextDisplayService<TSecurityContext>(Func<TSecurityContext, string> toStringFunc)
    : ISecurityContextDisplayService<TSecurityContext>
{
    public string ToString(TSecurityContext securityContext) => toStringFunc(securityContext);
}
