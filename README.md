# Course Registration API

A REST API for managing courses, institutional core goals, course-to-goal assignments, and semester offerings. The project demonstrates layered ASP.NET Core design, dependency injection, input validation, standardized error responses, MySQL persistence, and automated testing.

## Highlights

- Course create, read, update, and delete operations
- Core-goal management and course assignments
- Offering searches by semester, core goal, and department
- Case-insensitive, whitespace-normalized identifiers
- RFC 9457-style problem details for validation, conflict, and missing-resource responses
- In-memory storage for an immediate zero-configuration demo
- Optional MySQL persistence with parameterized asynchronous queries
- Service tests and HTTP integration tests
- GitHub Actions build and test workflow

## Portfolio case study

The source code here is paired with a portfolio case study that explains how the project grew from basic CRUD endpoints into a layered API architecture built around repositories, services, controllers, relational data, testing, and user stories.

[Read **From CRUD Endpoints to an API Architecture**](https://mystorageaccountusasa.z13.web.core.windows.net/projects/crud-to-api-architecture.html)

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Optional: MySQL 8 or a compatible server

## Run locally

The default in-memory mode includes a small sample data set and does not require a database.

```bash
dotnet restore tests/CourseRegistration.Api.Tests/CourseRegistration.Api.Tests.csproj
dotnet run --project src/CourseRegistration.Api/CourseRegistration.Api.csproj
```

The API listens on `http://localhost:5080` when launched with the included development profile.

## Run the checks

```bash
dotnet build tests/CourseRegistration.Api.Tests/CourseRegistration.Api.Tests.csproj --configuration Release
dotnet test tests/CourseRegistration.Api.Tests/CourseRegistration.Api.Tests.csproj --configuration Release --no-build
```

## Endpoints

| Method | Path | Purpose |
| --- | --- | --- |
| `GET` | `/api/courses` | List courses |
| `GET` | `/api/courses/{name}` | Get one course |
| `POST` | `/api/courses` | Create a course |
| `PUT` | `/api/courses/{name}` | Update a course |
| `DELETE` | `/api/courses/{name}` | Delete a course |
| `GET` | `/api/core-goals` | List core goals |
| `GET` | `/api/core-goals/{id}?includeCourses=true` | Get a core goal, optionally with assigned courses |
| `POST` | `/api/core-goals` | Create a core goal |
| `PUT` | `/api/core-goals/{id}` | Update a core goal |
| `POST` | `/api/core-goals/{id}/courses` | Assign existing courses to a goal |
| `DELETE` | `/api/core-goals/{id}` | Delete a core goal and its assignments |
| `GET` | `/api/offerings?semester={semester}` | Find offerings, optionally filtered by `goalId` and `department` |

Course names, goal IDs, and department codes are normalized to uppercase. Leading, trailing, and repeated spaces in identifiers are removed.

## Examples

List the seeded courses:

```bash
curl http://localhost:5080/api/courses
```

Create a course:

```bash
curl -X POST http://localhost:5080/api/courses \
  -H "Content-Type: application/json" \
  -d '{
    "name": "CSCI 495",
    "title": "Software Engineering Project",
    "credits": 3,
    "description": "Team-based development of a production-oriented software project."
  }'
```

Filter Spring 2026 offerings to the CSCI department:

```bash
curl "http://localhost:5080/api/offerings?semester=Spring%202026&department=CSCI"
```

## Use MySQL

1. Create and seed the schema:

   ```bash
   mysql -u root -p < database/schema.sql
   ```

2. Supply the provider and connection string through environment variables. Do not commit database credentials.

   PowerShell:

   ```powershell
   $env:Storage__Provider = "MySql"
   $env:ConnectionStrings__CourseRegistration = "Server=localhost;Database=course_registration;User ID=YOUR_USER;Password=YOUR_PASSWORD"
   dotnet run --project src/CourseRegistration.Api/CourseRegistration.Api.csproj
   ```

The application uses a new pooled connection for each repository operation. All SQL values are passed as parameters, and multi-row course assignments run inside a transaction.

## Project structure

```text
src/CourseRegistration.Api/
  Controllers/       HTTP endpoints
  Data/              In-memory and MySQL repositories
  Exceptions/        Domain-specific failures
  Infrastructure/    Centralized API error handling
  Models/            Resources and request contracts
  Services/          Validation and application logic
tests/CourseRegistration.Api.Tests/
database/schema.sql
```

## Design notes

- In-memory data resets whenever the process restarts.
- The MySQL schema is supplied, but a MySQL server is not required for the default demo or automated test suite.
- Authentication and authorization are intentionally outside this project's current scope.
- Production deployments should use a secret manager or protected environment variables for the connection string.

## Project history

This standalone portfolio edition evolved from a CSCI 330 course project completed by Robert Pelot in Spring 2026. The original GitHub Classroom repository and its commit history remain under the CCU Computing organization; classroom scaffolding and superseded copies were intentionally omitted here.
