using System.ComponentModel.DataAnnotations;
using CourseRegistration.Api.Models;
using CourseRegistration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseRegistration.Api.Controllers;

[ApiController]
[Route("api/offerings")]
public sealed class OfferingsController(CourseRegistrationService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CourseOffering>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CourseOffering>>> GetAll(
        [FromQuery, Required] string semester,
        [FromQuery] string? goalId,
        [FromQuery] string? department,
        CancellationToken cancellationToken) =>
        Ok(await service.GetOfferingsAsync(semester, goalId, department, cancellationToken));
}

