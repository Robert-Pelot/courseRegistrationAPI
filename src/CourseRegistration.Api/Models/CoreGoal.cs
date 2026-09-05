namespace CourseRegistration.Api.Models;

public sealed record CoreGoal(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<Course>? Courses = null);

