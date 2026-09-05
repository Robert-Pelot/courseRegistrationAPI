using CourseRegistration.Api.Data;
using CourseRegistration.Api.Infrastructure;
using CourseRegistration.Api.Services;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddScoped<CourseRegistrationService>();

var storageProvider = builder.Configuration["Storage:Provider"] ?? "Memory";
if (storageProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
{
    var connectionString = builder.Configuration.GetConnectionString("CourseRegistration")
        ?? throw new InvalidOperationException(
            "ConnectionStrings:CourseRegistration is required when Storage:Provider is MySql.");

    builder.Services.AddSingleton(new MySqlDataSourceBuilder(connectionString).Build());
    builder.Services.AddScoped<ICourseRepository, MySqlCourseRepository>();
}
else if (storageProvider.Equals("Memory", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<ICourseRepository, InMemoryCourseRepository>();
}
else
{
    throw new InvalidOperationException("Storage:Provider must be either 'Memory' or 'MySql'.");
}

var app = builder.Build();

app.UseExceptionHandler();
app.MapControllers();
string[] endpointPaths = ["/api/courses", "/api/core-goals", "/api/offerings"];
app.MapGet("/", () => Results.Ok(new
{
    name = "Course Registration API",
    version = "1.0",
    endpoints = endpointPaths
}));

app.Run();

public partial class Program;
