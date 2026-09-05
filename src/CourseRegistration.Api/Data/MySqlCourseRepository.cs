using CourseRegistration.Api.Models;
using MySqlConnector;

namespace CourseRegistration.Api.Data;

public sealed class MySqlCourseRepository(MySqlDataSource dataSource) : ICourseRepository
{
    public async Task<IReadOnlyList<Course>> GetCoursesAsync(CancellationToken cancellationToken)
    {
        const string sql = "SELECT name, title, credits, description FROM courses ORDER BY name";
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var courses = new List<Course>();
        while (await reader.ReadAsync(cancellationToken))
        {
            courses.Add(ReadCourse(reader));
        }

        return courses;
    }

    public async Task<Course?> GetCourseAsync(string name, CancellationToken cancellationToken)
    {
        const string sql = "SELECT name, title, credits, description FROM courses WHERE name = @name";
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", name);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadCourse(reader) : null;
    }

    public async Task<Course> CreateCourseAsync(Course course, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO courses (name, title, credits, description)
            VALUES (@name, @title, @credits, @description)
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = BuildCourseCommand(sql, connection, course);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return course;
    }

    public async Task<bool> UpdateCourseAsync(Course course, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE courses
            SET title = @title, credits = @credits, description = @description
            WHERE name = @name
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = BuildCourseCommand(sql, connection, course);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteCourseAsync(string name, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM courses WHERE name = @name";
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", name);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IReadOnlyList<CoreGoal>> GetCoreGoalsAsync(CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name, description FROM core_goals ORDER BY id";
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var goals = new List<CoreGoal>();
        while (await reader.ReadAsync(cancellationToken))
        {
            goals.Add(ReadCoreGoal(reader));
        }

        return goals;
    }

    public async Task<CoreGoal?> GetCoreGoalAsync(
        string id,
        bool includeCourses,
        CancellationToken cancellationToken)
    {
        const string goalSql = "SELECT id, name, description FROM core_goals WHERE id = @id";
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(goalSql, connection);
        command.Parameters.AddWithValue("@id", id);

        CoreGoal? goal;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            goal = await reader.ReadAsync(cancellationToken) ? ReadCoreGoal(reader) : null;
        }

        if (goal is null || !includeCourses)
        {
            return goal;
        }

        const string coursesSql = """
            SELECT c.name, c.title, c.credits, c.description
            FROM courses c
            INNER JOIN core_goal_courses gc ON gc.course_name = c.name
            WHERE gc.goal_id = @id
            ORDER BY c.name
            """;
        await using var coursesCommand = new MySqlCommand(coursesSql, connection);
        coursesCommand.Parameters.AddWithValue("@id", id);
        await using var coursesReader = await coursesCommand.ExecuteReaderAsync(cancellationToken);
        var courses = new List<Course>();
        while (await coursesReader.ReadAsync(cancellationToken))
        {
            courses.Add(ReadCourse(coursesReader));
        }

        return goal with { Courses = courses };
    }

    public async Task<CoreGoal> CreateCoreGoalAsync(CoreGoal goal, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO core_goals (id, name, description)
            VALUES (@id, @name, @description)
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = BuildCoreGoalCommand(sql, connection, goal);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return goal;
    }

    public async Task<bool> UpdateCoreGoalAsync(CoreGoal goal, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE core_goals SET name = @name, description = @description
            WHERE id = @id
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = BuildCoreGoalCommand(sql, connection, goal);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteCoreGoalAsync(string id, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM core_goals WHERE id = @id";
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task AssignCoursesAsync(
        string goalId,
        IReadOnlyList<string> courseNames,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT IGNORE INTO core_goal_courses (goal_id, course_name)
            VALUES (@goalId, @courseName)
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        foreach (var courseName in courseNames)
        {
            await using var command = new MySqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@goalId", goalId);
            command.Parameters.AddWithValue("@courseName", courseName);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseOffering>> GetOfferingsAsync(
        string semester,
        string? goalId,
        string? department,
        CancellationToken cancellationToken)
    {
        var sql = """
            SELECT c.name, c.title, c.credits, c.description, o.semester, o.section
            FROM course_offerings o
            INNER JOIN courses c ON c.name = o.course_name
            """;

        if (goalId is not null)
        {
            sql += " INNER JOIN core_goal_courses gc ON gc.course_name = c.name";
        }

        sql += " WHERE o.semester = @semester";
        if (goalId is not null)
        {
            sql += " AND gc.goal_id = @goalId";
        }

        if (department is not null)
        {
            sql += " AND c.name LIKE CONCAT(@department, ' %')";
        }

        sql += " ORDER BY c.name, o.section";

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@semester", semester);
        if (goalId is not null)
        {
            command.Parameters.AddWithValue("@goalId", goalId);
        }

        if (department is not null)
        {
            command.Parameters.AddWithValue("@department", department);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var offerings = new List<CourseOffering>();
        while (await reader.ReadAsync(cancellationToken))
        {
            offerings.Add(new CourseOffering(
                ReadCourse(reader),
                reader.GetString("semester"),
                reader.GetString("section")));
        }

        return offerings;
    }

    private static Course ReadCourse(MySqlDataReader reader) => new(
        reader.GetString("name"),
        reader.GetString("title"),
        reader.GetDecimal("credits"),
        reader.GetString("description"));

    private static CoreGoal ReadCoreGoal(MySqlDataReader reader) => new(
        reader.GetString("id"),
        reader.GetString("name"),
        reader.GetString("description"));

    private static MySqlCommand BuildCourseCommand(string sql, MySqlConnection connection, Course course)
    {
        var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", course.Name);
        command.Parameters.AddWithValue("@title", course.Title);
        command.Parameters.AddWithValue("@credits", course.Credits);
        command.Parameters.AddWithValue("@description", course.Description);
        return command;
    }

    private static MySqlCommand BuildCoreGoalCommand(string sql, MySqlConnection connection, CoreGoal goal)
    {
        var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", goal.Id);
        command.Parameters.AddWithValue("@name", goal.Name);
        command.Parameters.AddWithValue("@description", goal.Description);
        return command;
    }
}

