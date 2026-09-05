using System.Net;
using System.Net.Http.Json;
using CourseRegistration.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CourseRegistration.Api.Tests;

public sealed class ApiIntegrationTests : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();

    [Fact]
    public async Task GetCourses_ReturnsSeededCourses()
    {
        using var client = _factory.CreateClient();

        var courses = await client.GetFromJsonAsync<Course[]>(
            "/api/courses",
            TestContext.Current.CancellationToken);

        Assert.NotNull(courses);
        Assert.Contains(courses, course => course.Name == "CSCI 330");
    }

    [Fact]
    public async Task GetCourse_IsCaseInsensitive()
    {
        using var client = _factory.CreateClient();

        var course = await client.GetFromJsonAsync<Course>(
            "/api/courses/csci%20330",
            TestContext.Current.CancellationToken);

        Assert.NotNull(course);
        Assert.Equal("CSCI 330", course.Name);
    }

    [Fact]
    public async Task GetMissingCourse_ReturnsProblemDetails()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(
            "/api/courses/NONE%20999",
            TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Resource not found", problem?.Title);
    }

    [Fact]
    public async Task CreateCourse_ReturnsCreatedResource()
    {
        using var client = _factory.CreateClient();
        var request = new CreateCourseRequest("test 450", "Integration Testing", 3, "API test course");

        var response = await client.PostAsJsonAsync(
            "/api/courses",
            request,
            TestContext.Current.CancellationToken);
        var course = await response.Content.ReadFromJsonAsync<Course>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("TEST 450", course?.Name);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateInvalidCourse_ReturnsValidationProblem()
    {
        using var client = _factory.CreateClient();
        var request = new CreateCourseRequest("A", "", 0, "");

        var response = await client.PostAsJsonAsync(
            "/api/courses",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FilterOfferings_ReturnsMatchingDepartment()
    {
        using var client = _factory.CreateClient();

        var offerings = await client.GetFromJsonAsync<CourseOffering[]>(
            "/api/offerings?semester=Spring%202026&department=CSCI",
            TestContext.Current.CancellationToken);

        Assert.NotNull(offerings);
        Assert.Equal(2, offerings.Length);
        Assert.All(offerings, offering => Assert.StartsWith("CSCI ", offering.Course.Name));
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
