using CourseRegistration.Api.Models;

namespace CourseRegistration.Api.Data;

public sealed class InMemoryCourseRepository : ICourseRepository
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, Course> _courses = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CoreGoal> _goals = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _goalCourses = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string CourseName, string Semester, string Section)> _offerings = [];

    public InMemoryCourseRepository()
    {
        Seed();
    }

    public Task<IReadOnlyList<Course>> GetCoursesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            IReadOnlyList<Course> courses = _courses.Values.OrderBy(course => course.Name).ToArray();
            return Task.FromResult(courses);
        }
    }

    public Task<Course?> GetCourseAsync(string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _courses.TryGetValue(name, out var course);
            return Task.FromResult(course);
        }
    }

    public Task<Course> CreateCourseAsync(Course course, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _courses.Add(course.Name, course);
            return Task.FromResult(course);
        }
    }

    public Task<bool> UpdateCourseAsync(Course course, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (!_courses.ContainsKey(course.Name))
            {
                return Task.FromResult(false);
            }

            _courses[course.Name] = course;
            return Task.FromResult(true);
        }
    }

    public Task<bool> DeleteCourseAsync(string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (!_courses.Remove(name))
            {
                return Task.FromResult(false);
            }

            foreach (var courseNames in _goalCourses.Values)
            {
                courseNames.Remove(name);
            }

            _offerings.RemoveAll(offering => string.Equals(offering.CourseName, name, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(true);
        }
    }

    public Task<IReadOnlyList<CoreGoal>> GetCoreGoalsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            IReadOnlyList<CoreGoal> goals = _goals.Values.OrderBy(goal => goal.Id).ToArray();
            return Task.FromResult(goals);
        }
    }

    public Task<CoreGoal?> GetCoreGoalAsync(string id, bool includeCourses, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (!_goals.TryGetValue(id, out var goal))
            {
                return Task.FromResult<CoreGoal?>(null);
            }

            if (!includeCourses)
            {
                return Task.FromResult<CoreGoal?>(goal);
            }

            var courses = _goalCourses.TryGetValue(id, out var courseNames)
                ? courseNames.Select(name => _courses[name]).OrderBy(course => course.Name).ToArray()
                : [];

            return Task.FromResult<CoreGoal?>(goal with { Courses = courses });
        }
    }

    public Task<CoreGoal> CreateCoreGoalAsync(CoreGoal goal, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _goals.Add(goal.Id, goal);
            _goalCourses[goal.Id] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult(goal);
        }
    }

    public Task<bool> UpdateCoreGoalAsync(CoreGoal goal, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (!_goals.ContainsKey(goal.Id))
            {
                return Task.FromResult(false);
            }

            _goals[goal.Id] = goal;
            return Task.FromResult(true);
        }
    }

    public Task<bool> DeleteCoreGoalAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _goalCourses.Remove(id);
            return Task.FromResult(_goals.Remove(id));
        }
    }

    public Task AssignCoursesAsync(
        string goalId,
        IReadOnlyList<string> courseNames,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            var assigned = _goalCourses[goalId];
            foreach (var courseName in courseNames)
            {
                assigned.Add(courseName);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CourseOffering>> GetOfferingsAsync(
        string semester,
        string? goalId,
        string? department,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            IEnumerable<(string CourseName, string Semester, string Section)> query = _offerings.Where(
                offering => string.Equals(offering.Semester, semester, StringComparison.OrdinalIgnoreCase));

            if (goalId is not null)
            {
                var courseNames = _goalCourses.TryGetValue(goalId, out var assigned)
                    ? assigned
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                query = query.Where(offering => courseNames.Contains(offering.CourseName));
            }

            if (department is not null)
            {
                query = query.Where(offering =>
                    offering.CourseName.StartsWith(department + " ", StringComparison.OrdinalIgnoreCase));
            }

            IReadOnlyList<CourseOffering> offerings = query
                .Select(offering => new CourseOffering(
                    _courses[offering.CourseName],
                    offering.Semester,
                    offering.Section))
                .OrderBy(offering => offering.Course.Name)
                .ThenBy(offering => offering.Section)
                .ToArray();

            return Task.FromResult(offerings);
        }
    }

    private void Seed()
    {
        var courses = new[]
        {
            new Course("CSCI 330", "Software Engineering", 3, "Software design, construction, testing, and maintenance."),
            new Course("CSCI 434", "Digital Forensics", 3, "Methods and tools used to investigate digital evidence."),
            new Course("ENGL 102", "Composition and Critical Reading", 3, "Research, argumentation, and academic writing.")
        };

        foreach (var course in courses)
        {
            _courses.Add(course.Name, course);
        }

        var goal = new CoreGoal("CG1", "Critical Thinking", "Analyze information and develop evidence-based conclusions.");
        _goals.Add(goal.Id, goal);
        _goalCourses.Add(goal.Id, new HashSet<string>(["CSCI 330", "ENGL 102"], StringComparer.OrdinalIgnoreCase));

        _offerings.AddRange(
        [
            ("CSCI 330", "Spring 2026", "01"),
            ("CSCI 434", "Spring 2026", "01"),
            ("ENGL 102", "Spring 2026", "04")
        ]);
    }
}

