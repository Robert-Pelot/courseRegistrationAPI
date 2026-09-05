using System.ComponentModel.DataAnnotations;

namespace CourseRegistration.Api.Models;

public sealed record CreateCourseRequest(
    [param: Required, StringLength(20, MinimumLength = 3)] string Name,
    [param: Required, StringLength(200)] string Title,
    [param: Range(0.5, 12)] decimal Credits,
    [param: Required, StringLength(2_000)] string Description);

public sealed record UpdateCourseRequest(
    [param: Required, StringLength(200)] string Title,
    [param: Range(0.5, 12)] decimal Credits,
    [param: Required, StringLength(2_000)] string Description);

public sealed record CoreGoalRequest(
    [param: Required, StringLength(20)] string Id,
    [param: Required, StringLength(200)] string Name,
    [param: Required, StringLength(2_000)] string Description);

public sealed record UpdateCoreGoalRequest(
    [param: Required, StringLength(200)] string Name,
    [param: Required, StringLength(2_000)] string Description);

public sealed record AssignCoursesRequest(
    [param: Required, MinLength(1)] IReadOnlyList<string> CourseNames);
