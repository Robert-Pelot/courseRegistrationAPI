using CourseRegistration.Api.Models;

namespace CourseRegistration.Api.Data;

public interface ICourseRepository
{
    Task<IReadOnlyList<Course>> GetCoursesAsync(CancellationToken cancellationToken);
    Task<Course?> GetCourseAsync(string name, CancellationToken cancellationToken);
    Task<Course> CreateCourseAsync(Course course, CancellationToken cancellationToken);
    Task<bool> UpdateCourseAsync(Course course, CancellationToken cancellationToken);
    Task<bool> DeleteCourseAsync(string name, CancellationToken cancellationToken);

    Task<IReadOnlyList<CoreGoal>> GetCoreGoalsAsync(CancellationToken cancellationToken);
    Task<CoreGoal?> GetCoreGoalAsync(string id, bool includeCourses, CancellationToken cancellationToken);
    Task<CoreGoal> CreateCoreGoalAsync(CoreGoal goal, CancellationToken cancellationToken);
    Task<bool> UpdateCoreGoalAsync(CoreGoal goal, CancellationToken cancellationToken);
    Task<bool> DeleteCoreGoalAsync(string id, CancellationToken cancellationToken);
    Task AssignCoursesAsync(string goalId, IReadOnlyList<string> courseNames, CancellationToken cancellationToken);

    Task<IReadOnlyList<CourseOffering>> GetOfferingsAsync(
        string semester,
        string? goalId,
        string? department,
        CancellationToken cancellationToken);
}

