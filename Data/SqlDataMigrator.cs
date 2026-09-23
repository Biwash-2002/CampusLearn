using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CampusLearn.Models;

namespace CampusLearn.Data;

public class DataMigrationResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, int> SqliteCounts { get; set; } = new();
    public Dictionary<string, int> SqlServerCounts { get; set; } = new();
}

public static class SqlDataMigrator
{
    public static async Task<DataMigrationResult> MigrateSqliteToSqlServerAsync(
        string sqliteDbPath, 
        ApplicationDbContext targetContext, 
        ILogger? logger = null)
    {
        var result = new DataMigrationResult();

        if (!File.Exists(sqliteDbPath))
        {
            result.Succeeded = false;
            result.Message = $"Source SQLite database file not found at '{sqliteDbPath}'.";
            logger?.LogWarning(result.Message);
            return result;
        }

        var sourceOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={sqliteDbPath}")
            .Options;

        using var sourceContext = new ApplicationDbContext(sourceOptions);

        // Safeguard: Check if target database is already populated
        var existingUsersCount = await targetContext.Users.CountAsync();
        var existingCoursesCount = await targetContext.Courses.CountAsync();

        if (existingUsersCount > 0 || existingCoursesCount > 0)
        {
            result.Succeeded = true;
            result.Message = "Target SQL Server database is already populated. Skipped data migration to prevent overwriting existing data.";
            logger?.LogInformation(result.Message);

            // Record current counts
            result.SqlServerCounts["AspNetUsers"] = existingUsersCount;
            result.SqlServerCounts["Courses"] = existingCoursesCount;
            return result;
        }

        logger?.LogInformation("Starting safe data migration from SQLite to SQL Server...");

        using var transaction = await targetContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Roles
            var roles = await sourceContext.Roles.AsNoTracking().ToListAsync();
            result.SqliteCounts["AspNetRoles"] = roles.Count;
            if (roles.Count > 0)
            {
                await targetContext.Roles.AddRangeAsync(roles);
                await targetContext.SaveChangesAsync();
            }

            // 2. Users
            var users = await sourceContext.Users.AsNoTracking().ToListAsync();
            result.SqliteCounts["AspNetUsers"] = users.Count;
            if (users.Count > 0)
            {
                await targetContext.Users.AddRangeAsync(users);
                await targetContext.SaveChangesAsync();
            }

            // 3. UserRoles
            var userRoles = await sourceContext.UserRoles.AsNoTracking().ToListAsync();
            result.SqliteCounts["AspNetUserRoles"] = userRoles.Count;
            if (userRoles.Count > 0)
            {
                await targetContext.UserRoles.AddRangeAsync(userRoles);
                await targetContext.SaveChangesAsync();
            }

            // 4. Courses
            var courses = await sourceContext.Courses.AsNoTracking().ToListAsync();
            result.SqliteCounts["Courses"] = courses.Count;
            if (courses.Count > 0)
            {
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Courses ON");
                }
                await targetContext.Courses.AddRangeAsync(courses);
                await targetContext.SaveChangesAsync();
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Courses OFF");
                }
            }

            // 5. Lessons
            var lessons = await sourceContext.Lessons.AsNoTracking().ToListAsync();
            result.SqliteCounts["Lessons"] = lessons.Count;
            if (lessons.Count > 0)
            {
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Lessons ON");
                }
                await targetContext.Lessons.AddRangeAsync(lessons);
                await targetContext.SaveChangesAsync();
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Lessons OFF");
                }
            }

            // 6. Quizzes
            var quizzes = await sourceContext.Quizzes.AsNoTracking().ToListAsync();
            result.SqliteCounts["Quizzes"] = quizzes.Count;
            if (quizzes.Count > 0)
            {
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Quizzes ON");
                }
                await targetContext.Quizzes.AddRangeAsync(quizzes);
                await targetContext.SaveChangesAsync();
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Quizzes OFF");
                }
            }

            // 7. Questions
            var questions = await sourceContext.Questions.AsNoTracking().ToListAsync();
            result.SqliteCounts["Questions"] = questions.Count;
            if (questions.Count > 0)
            {
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Questions ON");
                }
                await targetContext.Questions.AddRangeAsync(questions);
                await targetContext.SaveChangesAsync();
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Questions OFF");
                }
            }

            // 8. Enrollments
            var enrollments = await sourceContext.Enrollments.AsNoTracking().ToListAsync();
            result.SqliteCounts["Enrollments"] = enrollments.Count;
            if (enrollments.Count > 0)
            {
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Enrollments ON");
                }
                await targetContext.Enrollments.AddRangeAsync(enrollments);
                await targetContext.SaveChangesAsync();
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Enrollments OFF");
                }
            }

            // 9. LessonProgresses
            var lessonProgresses = await sourceContext.LessonProgresses.AsNoTracking().ToListAsync();
            result.SqliteCounts["LessonProgresses"] = lessonProgresses.Count;
            if (lessonProgresses.Count > 0)
            {
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT LessonProgresses ON");
                }
                await targetContext.LessonProgresses.AddRangeAsync(lessonProgresses);
                await targetContext.SaveChangesAsync();
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT LessonProgresses OFF");
                }
            }

            // 10. QuizResults
            var quizResults = await sourceContext.QuizResults.AsNoTracking().ToListAsync();
            result.SqliteCounts["QuizResults"] = quizResults.Count;
            if (quizResults.Count > 0)
            {
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT QuizResults ON");
                }
                await targetContext.QuizResults.AddRangeAsync(quizResults);
                await targetContext.SaveChangesAsync();
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT QuizResults OFF");
                }
            }

            // 11. ContactMessages
            var contactMessages = await sourceContext.ContactMessages.AsNoTracking().ToListAsync();
            result.SqliteCounts["ContactMessages"] = contactMessages.Count;
            if (contactMessages.Count > 0)
            {
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT ContactMessages ON");
                }
                await targetContext.ContactMessages.AddRangeAsync(contactMessages);
                await targetContext.SaveChangesAsync();
                if (targetContext.Database.IsSqlServer())
                {
                    await targetContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT ContactMessages OFF");
                }
            }

            await transaction.CommitAsync();

            // Populate target counts
            result.SqlServerCounts["AspNetRoles"] = await targetContext.Roles.CountAsync();
            result.SqlServerCounts["AspNetUsers"] = await targetContext.Users.CountAsync();
            result.SqlServerCounts["AspNetUserRoles"] = await targetContext.UserRoles.CountAsync();
            result.SqlServerCounts["Courses"] = await targetContext.Courses.CountAsync();
            result.SqlServerCounts["Lessons"] = await targetContext.Lessons.CountAsync();
            result.SqlServerCounts["Quizzes"] = await targetContext.Quizzes.CountAsync();
            result.SqlServerCounts["Questions"] = await targetContext.Questions.CountAsync();
            result.SqlServerCounts["Enrollments"] = await targetContext.Enrollments.CountAsync();
            result.SqlServerCounts["LessonProgresses"] = await targetContext.LessonProgresses.CountAsync();
            result.SqlServerCounts["QuizResults"] = await targetContext.QuizResults.CountAsync();
            result.SqlServerCounts["ContactMessages"] = await targetContext.ContactMessages.CountAsync();

            result.Succeeded = true;
            result.Message = "Data successfully migrated from SQLite to SQL Server with all entities and foreign-key relations preserved.";
            logger?.LogInformation(result.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Succeeded = false;
            result.Message = $"Data migration failed: {ex.Message}";
            logger?.LogError(ex, "Error during SQLite to SQL Server data migration.");
        }

        return result;
    }
}
