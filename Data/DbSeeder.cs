using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CampusLearn.Models;

namespace CampusLearn.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("CampusLearn.Data.DbSeeder");

        // If target is SQL Server, automatically migrate existing SQLite data if fresh
        if (context.Database.IsSqlServer() && File.Exists("CampusLearn.db"))
        {
            var migrationResult = await SqlDataMigrator.MigrateSqliteToSqlServerAsync("CampusLearn.db", context, logger);
            if (migrationResult.Succeeded)
            {
                logger?.LogInformation("SQLite to SQL Server migration check: {Message}", migrationResult.Message);
            }
        }

        // 1. Seed Roles (Idempotent)
        string[] roles = { "Administrator", "Learner" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger?.LogInformation("Created role: {Role}", role);
            }
        }

        // 2. Seed Administrator User (Configuration/Environment-based)
        var adminEmail = configuration["AdminUser:Email"] ?? "admin@campuslearn.com";
        var adminPassword = configuration["AdminUser:Password"];
        var adminFullName = configuration["AdminUser:FullName"] ?? "System Administrator";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            if (!string.IsNullOrWhiteSpace(adminPassword))
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                    logger?.LogInformation("Successfully seeded administrator account: {Email}", adminEmail);
                }
                else
                {
                    logger?.LogError("Failed to create administrator account: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger?.LogInformation("Administrator user account was not created because 'AdminUser:Password' was not specified in configuration.");
            }
        }
        else
        {
            // Ensure the administrator has the Administrator role
            if (!await userManager.IsInRoleAsync(adminUser, "Administrator"))
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
                logger?.LogInformation("Assigned Administrator role to existing user: {Email}", adminEmail);
            }

            // If an explicit administrator password is provided in environment variables / configuration, synchronize it
            if (!string.IsNullOrWhiteSpace(adminPassword))
            {
                var isPasswordValid = await userManager.CheckPasswordAsync(adminUser, adminPassword);
                if (!isPasswordValid)
                {
                    if (await userManager.HasPasswordAsync(adminUser))
                    {
                        await userManager.RemovePasswordAsync(adminUser);
                    }
                    var addResult = await userManager.AddPasswordAsync(adminUser, adminPassword);
                    if (addResult.Succeeded)
                    {
                        // Reset lockout state when password is explicitly updated via configuration
                        await userManager.SetLockoutEndDateAsync(adminUser, null);
                        await userManager.ResetAccessFailedCountAsync(adminUser);
                        logger?.LogInformation("Synchronized administrator credentials for: {Email}", adminEmail);
                    }
                    else
                    {
                        logger?.LogError("Failed to synchronize administrator credentials: {Errors}",
                            string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    }
                }
            }
        }

        // 3. Seed Sample Courses
        if (!context.Courses.Any())
        {
            var courses = new List<Course>
            {
                new Course
                {
                    Title = "Introduction to Web Development",
                    Description = "Learn the fundamentals of web development including HTML, CSS, and JavaScript. This course covers the building blocks of modern web applications, from structuring content with HTML to styling with CSS and adding interactivity with JavaScript.",
                    Category = "Web Development",
                    Duration = "6 Weeks",
                    ImageUrl = "/images/courses/web-dev.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Lessons = new List<Lesson>
                    {
                        new Lesson
                        {
                            Title = "Getting Started with HTML",
                            Content = "<h2>What is HTML?</h2><p>HTML (HyperText Markup Language) is the standard markup language for creating web pages. It describes the structure of a web page using a series of elements.</p><h3>Basic HTML Structure</h3><pre><code>&lt;!DOCTYPE html&gt;\n&lt;html&gt;\n&lt;head&gt;\n    &lt;title&gt;My First Page&lt;/title&gt;\n&lt;/head&gt;\n&lt;body&gt;\n    &lt;h1&gt;Hello World&lt;/h1&gt;\n    &lt;p&gt;This is my first web page.&lt;/p&gt;\n&lt;/body&gt;\n&lt;/html&gt;</code></pre><h3>Key HTML Elements</h3><ul><li><strong>Headings:</strong> h1 through h6 for titles and subtitles</li><li><strong>Paragraphs:</strong> p for text content</li><li><strong>Links:</strong> a for hyperlinks</li><li><strong>Images:</strong> img for displaying images</li><li><strong>Lists:</strong> ul, ol, and li for lists</li></ul>",
                            LessonOrder = 1,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Lesson
                        {
                            Title = "CSS Fundamentals",
                            Content = "<h2>Introduction to CSS</h2><p>CSS (Cascading Style Sheets) is used to style and layout web pages. It controls the visual presentation of HTML elements.</p><h3>CSS Syntax</h3><pre><code>selector {\n    property: value;\n}</code></pre><h3>Ways to Add CSS</h3><ol><li><strong>Inline:</strong> Using the style attribute</li><li><strong>Internal:</strong> Using a style tag in the head</li><li><strong>External:</strong> Linking to a separate CSS file</li></ol><h3>Common CSS Properties</h3><ul><li><strong>color:</strong> Text color</li><li><strong>background-color:</strong> Background color</li><li><strong>font-size:</strong> Size of text</li><li><strong>margin:</strong> Space outside an element</li><li><strong>padding:</strong> Space inside an element</li></ul>",
                            LessonOrder = 2,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Lesson
                        {
                            Title = "JavaScript Basics",
                            Content = "<h2>JavaScript Introduction</h2><p>JavaScript is a programming language that enables interactive web pages. It is an essential part of web applications, alongside HTML and CSS.</p><h3>Variables and Data Types</h3><pre><code>let name = \"John\";\nconst age = 25;\nlet isStudent = true;\nlet scores = [95, 88, 72];\nlet person = { name: \"John\", age: 25 };</code></pre><h3>Functions</h3><pre><code>function greet(name) {\n    return \"Hello, \" + name + \"!\";\n}\n\n// Arrow function\nconst add = (a, b) =&gt; a + b;</code></pre>",
                            LessonOrder = 3,
                            CreatedAt = DateTime.UtcNow
                        }
                    },
                    Quizzes = new List<Quiz>
                    {
                        new Quiz
                        {
                            Title = "Web Development Fundamentals Quiz",
                            Description = "Test your knowledge of HTML, CSS, and JavaScript basics.",
                            Questions = new List<Question>
                            {
                                new Question { QuestionText = "What does HTML stand for?", OptionA = "Hyper Text Markup Language", OptionB = "High Tech Modern Language", OptionC = "Hyper Transfer Markup Language", OptionD = "Home Tool Markup Language", CorrectAnswer = "A" },
                                new Question { QuestionText = "Which tag is used for the largest heading in HTML?", OptionA = "<heading>", OptionB = "<h6>", OptionC = "<h1>", OptionD = "<head>", CorrectAnswer = "C" },
                                new Question { QuestionText = "What does CSS stand for?", OptionA = "Creative Style Sheets", OptionB = "Cascading Style Sheets", OptionC = "Computer Style Sheets", OptionD = "Colorful Style Sheets", CorrectAnswer = "B" },
                                new Question { QuestionText = "Which keyword is used to declare a constant variable in JavaScript?", OptionA = "var", OptionB = "let", OptionC = "const", OptionD = "fixed", CorrectAnswer = "C" }
                            }
                        }
                    }
                },
                new Course
                {
                    Title = "Object-Oriented Programming with C#",
                    Description = "Master the core concepts of Object-Oriented Programming using C# and .NET. Learn about classes, objects, inheritance, polymorphism, interfaces, exception handling, and modern C# features.",
                    Category = "Programming",
                    Duration = "8 Weeks",
                    ImageUrl = "/images/courses/csharp-oop.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Lessons = new List<Lesson>
                    {
                        new Lesson
                        {
                            Title = "Classes and Objects",
                            Content = "<h2>Understanding Classes and Objects</h2><p>In C#, a class is a blueprint for creating objects. A class defines properties and methods that the created objects will have.</p><h3>Defining a Class</h3><pre><code>public class Student\n{\n    public string Name { get; set; }\n    public int Age { get; set; }\n    \n    public void Study()\n    {\n        Console.WriteLine($\"{Name} is studying.\");\n    }\n}</code></pre><h3>Creating an Object</h3><pre><code>Student s = new Student { Name = \"Alice\", Age = 20 };\ns.Study();</code></pre>",
                            LessonOrder = 1,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Lesson
                        {
                            Title = "Inheritance and Polymorphism",
                            Content = "<h2>Inheritance</h2><p>Inheritance allows a class to inherit members from a base class, promoting code reuse.</p><h3>Example</h3><pre><code>public class Person\n{\n    public string Name { get; set; }\n    public virtual void Introduce()\n    {\n        Console.WriteLine($\"Hi, I am {Name}\");\n    }\n}\n\npublic class Teacher : Person\n{\n    public string Subject { get; set; }\n    public override void Introduce()\n    {\n        Console.WriteLine($\"Hi, I am {Name} and I teach {Subject}\");\n    }\n}</code></pre>",
                            LessonOrder = 2,
                            CreatedAt = DateTime.UtcNow
                        }
                    },
                    Quizzes = new List<Quiz>
                    {
                        new Quiz
                        {
                            Title = "C# OOP Mastery Quiz",
                            Description = "Assessment of classes, objects, inheritance, and polymorphism concepts.",
                            Questions = new List<Question>
                            {
                                new Question { QuestionText = "Which keyword is used to inherit from a base class in C#?", OptionA = "extends", OptionB = ":", OptionC = "implements", OptionD = "inherits", CorrectAnswer = "B" },
                                new Question { QuestionText = "Which keyword allows a method to be overridden in a derived class?", OptionA = "static", OptionB = "override", OptionC = "virtual", OptionD = "abstract", CorrectAnswer = "C" }
                            }
                        }
                    }
                },
                new Course
                {
                    Title = "Database Design and SQL Essentials",
                    Description = "Comprehensive introduction to relational database management systems, normalization, SQL queries, entity-relationship diagrams, and indexing best practices.",
                    Category = "Databases",
                    Duration = "5 Weeks",
                    ImageUrl = "/images/courses/database-sql.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Lessons = new List<Lesson>
                    {
                        new Lesson
                        {
                            Title = "Relational Concepts and DDL",
                            Content = "<h2>Relational Databases</h2><p>A relational database organizes data into tables consisting of rows and columns, linked by keys.</p><h3>Basic SQL DDL</h3><pre><code>CREATE TABLE Students (\n    StudentId INT PRIMARY KEY IDENTITY,\n    FullName NVARCHAR(100) NOT NULL,\n    Email NVARCHAR(256) UNIQUE NOT NULL,\n    EnrollmentDate DATETIME DEFAULT GETUTCDATE()\n);</code></pre>",
                            LessonOrder = 1,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Lesson
                        {
                            Title = "Querying Data with SQL",
                            Content = "<h2>SELECT Queries and Joins</h2><p>Retrieve and join related datasets using SQL queries.</p><pre><code>SELECT s.FullName, c.Title, e.EnrollmentDate\nFROM Enrollments e\nJOIN Students s ON e.UserId = s.StudentId\nJOIN Courses c ON e.CourseId = c.CourseId\nWHERE e.Status = 'Active';</code></pre>",
                            LessonOrder = 2,
                            CreatedAt = DateTime.UtcNow
                        }
                    },
                    Quizzes = new List<Quiz>
                    {
                        new Quiz
                        {
                            Title = "SQL and Relational Database Quiz",
                            Description = "Test your understanding of tables, keys, and SQL queries.",
                            Questions = new List<Question>
                            {
                                new Question { QuestionText = "Which SQL clause is used to filter records?", OptionA = "GROUP BY", OptionB = "HAVING", OptionC = "WHERE", OptionD = "ORDER BY", CorrectAnswer = "C" },
                                new Question { QuestionText = "Which constraint uniquely identifies each record in a database table?", OptionA = "UNIQUE", OptionB = "PRIMARY KEY", OptionC = "FOREIGN KEY", OptionD = "CHECK", CorrectAnswer = "B" }
                            }
                        }
                    }
                }
            };

            context.Courses.AddRange(courses);
            await context.SaveChangesAsync();
        }
    }
}
