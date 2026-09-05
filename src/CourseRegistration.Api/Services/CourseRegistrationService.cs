using CourseRegistration.Api.Data;
using CourseRegistration.Api.Exceptions;
using CourseRegistration.Api.Models;

namespace CourseRegistration.Api.Services;

public sealed class CourseRegistrationService(ICourseRepository repository)
{
    public Task<IReadOnlyList<Course>> GetCoursesAsync(CancellationToken cancellationToken) =>
        repository.GetCoursesAsync(cancellationToken);

    public async Task<Course> GetCourseAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeCode(name, "Course name");
        return await repository.GetCourseAsync(normalizedName, cancellationToken)
            ?? throw new ResourceNotFoundException($"Course '{normalizedName}' was not found.");
    }

    public async Task<Course> CreateCourseAsync(
        CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var course = new Course(
            NormalizeCode(request.Name, "Course name"),
            NormalizeText(request.Title, "Title"),
            request.Credits,
            NormalizeText(request.Description, "Description"));

        if (await repository.GetCourseAsync(course.Name, cancellationToken) is not null)
        {
            throw new ResourceConflictException($"Course '{course.Name}' already exists.");
        }

        return await repository.CreateCourseAsync(course, cancellationToken);
    }

    public async Task<Course> UpdateCourseAsync(
        string name,
        UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeCode(name, "Course name");
        var updated = new Course(
            normalizedName,
            NormalizeText(request.Title, "Title"),
            request.Credits,
            NormalizeText(request.Description, "Description"));

        if (!await repository.UpdateCourseAsync(updated, cancellationToken))
        {
            throw new ResourceNotFoundException($"Course '{normalizedName}' was not found.");
        }

        return updated;
    }

    public async Task DeleteCourseAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeCode(name, "Course name");
        if (!await repository.DeleteCourseAsync(normalizedName, cancellationToken))
        {
            throw new ResourceNotFoundException($"Course '{normalizedName}' was not found.");
        }
    }

    public Task<IReadOnlyList<CoreGoal>> GetCoreGoalsAsync(CancellationToken cancellationToken) =>
        repository.GetCoreGoalsAsync(cancellationToken);

    public async Task<CoreGoal> GetCoreGoalAsync(
        string id,
        bool includeCourses,
        CancellationToken cancellationToken)
    {
        var normalizedId = NormalizeCode(id, "Core-goal ID");
        return await repository.GetCoreGoalAsync(normalizedId, includeCourses, cancellationToken)
            ?? throw new ResourceNotFoundException($"Core goal '{normalizedId}' was not found.");
    }

    public async Task<CoreGoal> CreateCoreGoalAsync(
        CoreGoalRequest request,
        CancellationToken cancellationToken)
    {
        var goal = new CoreGoal(
            NormalizeCode(request.Id, "Core-goal ID"),
            NormalizeText(request.Name, "Name"),
            NormalizeText(request.Description, "Description"));

        if (await repository.GetCoreGoalAsync(goal.Id, false, cancellationToken) is not null)
        {
            throw new ResourceConflictException($"Core goal '{goal.Id}' already exists.");
        }

        return await repository.CreateCoreGoalAsync(goal, cancellationToken);
    }

    public async Task<CoreGoal> UpdateCoreGoalAsync(
        string id,
        UpdateCoreGoalRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedId = NormalizeCode(id, "Core-goal ID");
        var goal = new CoreGoal(
            normalizedId,
            NormalizeText(request.Name, "Name"),
            NormalizeText(request.Description, "Description"));

        if (!await repository.UpdateCoreGoalAsync(goal, cancellationToken))
        {
            throw new ResourceNotFoundException($"Core goal '{normalizedId}' was not found.");
        }

        return goal;
    }

    public async Task DeleteCoreGoalAsync(string id, CancellationToken cancellationToken)
    {
        var normalizedId = NormalizeCode(id, "Core-goal ID");
        if (!await repository.DeleteCoreGoalAsync(normalizedId, cancellationToken))
        {
            throw new ResourceNotFoundException($"Core goal '{normalizedId}' was not found.");
        }
    }

    public async Task<CoreGoal> AssignCoursesAsync(
        string id,
        AssignCoursesRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedId = NormalizeCode(id, "Core-goal ID");
        _ = await GetCoreGoalAsync(normalizedId, false, cancellationToken);

        var courseNames = request.CourseNames
            .Select(name => NormalizeCode(name, "Course name"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (courseNames.Length == 0)
        {
            throw new RequestValidationException("At least one course name is required.");
        }

        foreach (var courseName in courseNames)
        {
            if (await repository.GetCourseAsync(courseName, cancellationToken) is null)
            {
                throw new RequestValidationException($"Course '{courseName}' does not exist.");
            }
        }

        await repository.AssignCoursesAsync(normalizedId, courseNames, cancellationToken);
        return await GetCoreGoalAsync(normalizedId, true, cancellationToken);
    }

    public async Task<IReadOnlyList<CourseOffering>> GetOfferingsAsync(
        string semester,
        string? goalId,
        string? department,
        CancellationToken cancellationToken)
    {
        var normalizedSemester = NormalizeText(semester, "Semester");
        var normalizedGoalId = string.IsNullOrWhiteSpace(goalId)
            ? null
            : NormalizeCode(goalId, "Core-goal ID");
        var normalizedDepartment = string.IsNullOrWhiteSpace(department)
            ? null
            : NormalizeCode(department, "Department");

        if (normalizedGoalId is not null)
        {
            _ = await GetCoreGoalAsync(normalizedGoalId, false, cancellationToken);
        }

        return await repository.GetOfferingsAsync(
            normalizedSemester,
            normalizedGoalId,
            normalizedDepartment,
            cancellationToken);
    }

    private static string NormalizeCode(string? value, string fieldName)
    {
        var normalized = NormalizeText(value, fieldName);
        return string.Join(' ', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
    }

    private static string NormalizeText(string? value, string fieldName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new RequestValidationException($"{fieldName} is required.");
        }

        return normalized;
    }
}

