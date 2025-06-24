namespace SecuritySystem.Providers;

public class AccessDeniedSecurityProvider<TDomainObject>() : ConstSecurityProvider<TDomainObject>(false);