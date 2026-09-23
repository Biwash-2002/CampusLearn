using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Models;

namespace CampusLearn.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<LessonProgress> LessonProgresses { get; set; }
    public DbSet<QuizResult> QuizResults { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Course entity configuration
        builder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.CourseId);

            // One Course has many Lessons
            entity.HasMany(c => c.Lessons)
                  .WithOne(l => l.Course)
                  .HasForeignKey(l => l.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            // One Course has many Enrollments
            entity.HasMany(c => c.Enrollments)
                  .WithOne(e => e.Course)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            // One Course has one or more Quizzes
            entity.HasMany(c => c.Quizzes)
                  .WithOne(q => q.Course)
                  .HasForeignKey(q => q.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Lesson entity configuration
        builder.Entity<Lesson>(entity =>
        {
            entity.HasKey(l => l.LessonId);

            // One Lesson has many LessonProgress records
            entity.HasMany(l => l.LessonProgresses)
                  .WithOne(lp => lp.Lesson)
                  .HasForeignKey(lp => lp.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Quiz entity configuration
        builder.Entity<Quiz>(entity =>
        {
            entity.HasKey(q => q.QuizId);

            // One Quiz has many Questions
            entity.HasMany(q => q.Questions)
                  .WithOne(quest => quest.Quiz)
                  .HasForeignKey(quest => quest.QuizId)
                  .OnDelete(DeleteBehavior.Cascade);

            // One Quiz has many QuizResults
            entity.HasMany(q => q.QuizResults)
                  .WithOne(qr => qr.Quiz)
                  .HasForeignKey(qr => qr.QuizId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Question entity configuration
        builder.Entity<Question>(entity =>
        {
            entity.HasKey(quest => quest.QuestionId);
        });

        // Enrollment entity configuration
        builder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId);

            // Prevent duplicate enrollment of the same learner into the same course
            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();

            // One Learner can have many Enrollments
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Enrollments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // One Enrollment has many LessonProgress records (Restrict to avoid SQL Server multiple cascade delete path Error 1785)
            entity.HasMany(e => e.LessonProgresses)
                  .WithOne(lp => lp.Enrollment)
                  .HasForeignKey(lp => lp.EnrollmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // LessonProgress entity configuration
        builder.Entity<LessonProgress>(entity =>
        {
            entity.HasKey(lp => lp.LessonProgressId);

            // Prevent duplicate lesson-progress records for the same learner and lesson
            entity.HasIndex(lp => new { lp.UserId, lp.LessonId }).IsUnique();

            // One Learner can have many LessonProgress records
            entity.HasOne(lp => lp.User)
                  .WithMany(u => u.LessonProgresses)
                  .HasForeignKey(lp => lp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // QuizResult entity configuration
        builder.Entity<QuizResult>(entity =>
        {
            entity.HasKey(qr => qr.QuizResultId);

            // One Learner can have many QuizResults
            entity.HasOne(qr => qr.User)
                  .WithMany(u => u.QuizResults)
                  .HasForeignKey(qr => qr.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ContactMessage configuration
        builder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(cm => cm.ContactMessageId);
        });
    }
}
