using CourseRegistration.Api.Models;
using CourseRegistration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseRegistration.Api.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController(CourseRegistrationService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Course>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Course>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetCoursesAsync(cancellationToken));

    [HttpGet("{name}")]
    [ProducesResponseType<Course>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Course>> GetByName(string name, CancellationToken cancellationToken) =>
        Ok(await service.GetCourseAsync(name, cancellationToken));

    [HttpPost]
    [ProducesResponseType<Course>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Course>> Create(
        CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var course = await service.CreateCourseAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByName), new { name = course.Name }, course);
    }

    [HttpPut("{name}")]
    [ProducesResponseType<Course>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Course>> Update(
        string name,
        UpdateCourseRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.UpdateCourseAsync(name, request, cancellationToken));

    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string name, CancellationToken cancellationToken)
    {
        await service.DeleteCourseAsync(name, cancellationToken);
        return NoContent();
    }
}

