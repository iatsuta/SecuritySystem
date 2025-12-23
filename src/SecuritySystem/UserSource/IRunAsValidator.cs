using SecuritySystem.Credential;
using SecuritySystem.Validation;

namespace SecuritySystem.UserSource;

public interface IRunAsValidator : ISecurityValidator<UserCredential>;