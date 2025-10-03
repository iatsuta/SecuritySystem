namespace ExampleApp.Api.Controllers;

public record BuDto(Guid Id, string Name, Guid? ParentId);