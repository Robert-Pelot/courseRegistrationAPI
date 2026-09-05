using CourseRegistration.Api.Data;
using CourseRegistration.Api.Exceptions;
using CourseRegistration.Api.Models;
using CourseRegistration.Api.Services;
using Xunit;

namespace CourseRegistration.Api.Tests;

public sealed class CourseRegistrationServiceTests
{
    [Fact]
    public async Task GetCourse_NormalizesNameAndIgnoresCase()
    {
        var service = CreateService();

        var course = await service.GetCourseAsync(" csci   330 ", TestContext.Current.CancellationToken);

        Assert.Equal("CSCI 330", course.Name);
    }

    [Fact]
    public async Task CreateCourse_NormalizesInput()
    {
        var service = CreateService();
        var request = new CreateCourseRequest(" test   101 ", "  Test Course  ", 3, "  Description  ");

        var course = await service.CreateCourseAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal("TEST 101", course.Name);
        Assert.Equal("Test Course", course.Title);
        Assert.Equal("Description", course.Description);
    }

    [Fact]
    public async Task CreateCourse_WhenNameExists_ThrowsConflict()
    {
        var service = CreateService();
        var request = new CreateCourseRequest("csci 330", "Duplicate", 3, "Duplicate course");

        await Assert.ThrowsAsync<ResourceConflictException>(
            () => service.CreateCourseAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UpdateCourse_WhenCourseDoesNotExist_ThrowsNotFound()
    {
        var service = CreateService();
        var request = new UpdateCourseRequest("Missing", 3, "Missing course");

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => service.UpdateCourseAsync("NONE 999", request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteCourse_RemovesCourse()
    {
        var service = CreateService();

        await service.DeleteCourseAsync("ENGL 102", TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => service.GetCourseAsync("ENGL 102", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AssignCourses_ReturnsGoalWithAssignedCourses()
    {
        var service = CreateService();
        var request = new AssignCoursesRequest(["CSCI 434", "csci 434"]);

        var goal = await service.AssignCoursesAsync("cg1", request, TestContext.Current.CancellationToken);

        Assert.NotNull(goal.Courses);
        Assert.Contains(goal.Courses, course => course.Name == "CSCI 434");
        Assert.Equal(3, goal.Courses.Count);
    }

    [Fact]
    public async Task AssignCourses_WhenCourseDoesNotExist_RejectsRequest()
    {
        var service = CreateService();
        var request = new AssignCoursesRequest(["NONE 999"]);

        await Assert.ThrowsAsync<RequestValidationException>(
            () => service.AssignCoursesAsync("CG1", request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetOfferings_CanFilterByDepartment()
    {
        var service = CreateService();

        var offerings = await service.GetOfferingsAsync(
            "Spring 2026",
            null,
            "csci",
            TestContext.Current.CancellationToken);

        Assert.Equal(2, offerings.Count);
        Assert.All(offerings, offering => Assert.StartsWith("CSCI ", offering.Course.Name));
    }

    private static CourseRegistrationService CreateService() =>
        new(new InMemoryCourseRepository());
}
