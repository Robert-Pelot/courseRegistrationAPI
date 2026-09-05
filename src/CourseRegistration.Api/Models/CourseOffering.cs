namespace CourseRegistration.Api.Models;

public sealed record CourseOffering(
    Course Course,
    string Semester,
    string Section);

