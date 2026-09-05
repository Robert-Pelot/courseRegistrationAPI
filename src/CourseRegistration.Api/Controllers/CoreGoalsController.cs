using CourseRegistration.Api.Models;
using CourseRegistration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseRegistration.Api.Controllers;

[ApiController]
[Route("api/core-goals")]
public sealed class CoreGoalsController(CourseRegistrationService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CoreGoal>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CoreGoal>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetCoreGoalsAsync(cancellationToken));

    [HttpGet("{id}")]
    [ProducesResponseType<CoreGoal>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoreGoal>> GetById(
        string id,
        [FromQuery] bool includeCourses,
        CancellationToken cancellationToken) =>
        Ok(await service.GetCoreGoalAsync(id, includeCourses, cancellationToken));

    [HttpPost]
    [ProducesResponseType<CoreGoal>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CoreGoal>> Create(
        CoreGoalRequest request,
        CancellationToken cancellationToken)
    {
        var goal = await service.CreateCoreGoalAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = goal.Id }, goal);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<CoreGoal>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoreGoal>> Update(
        string id,
        UpdateCoreGoalRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.UpdateCoreGoalAsync(id, request, cancellationToken));

    [HttpPost("{id}/courses")]
    [ProducesResponseType<CoreGoal>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoreGoal>> AssignCourses(
        string id,
        AssignCoursesRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.AssignCoursesAsync(id, request, cancellationToken));

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await service.DeleteCoreGoalAsync(id, cancellationToken);
        return NoContent();
    }
}

