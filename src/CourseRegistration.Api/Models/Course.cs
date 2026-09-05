namespace CourseRegistration.Api.Models;

public sealed record Course(
    string Name,
    string Title,
    decimal Credits,
    string Description);

