namespace CourseRegistration.Api.Exceptions;

public sealed class ResourceNotFoundException(string message) : Exception(message);

public sealed class ResourceConflictException(string message) : Exception(message);

public sealed class RequestValidationException(string message) : Exception(message);

