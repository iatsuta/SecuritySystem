using SecuritySystem.Credential;

namespace SecuritySystem.Services;

public interface IRunAsValidator : ISecurityValidator<UserCredential>;