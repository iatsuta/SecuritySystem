// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class AccessDeniedException(string message) : SecuritySystemException(message);
