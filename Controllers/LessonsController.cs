using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;
using CampusLearn.ViewModels;

namespace CampusLearn.Controllers;

[Authorize(Roles = "Learner")]
public class LessonsController : Controller
{
    private readonly ApplicationDbContext _context;

    public LessonsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Course)
                .ThenInclude(c => c.Lessons.OrderBy(ol => ol.LessonOrder))
            .FirstOrDefaultAsync(l => l.LessonId == id);

        if (lesson == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == lesson.CourseId && e.UserId == userId);

        if (enrollment == null)
        {
            TempData["ErrorMessage"] = "You must be enrolled in this course to access its lessons.";
            return RedirectToAction("Details", "Courses", new { id = lesson.CourseId });
        }

        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.LessonId == id && lp.UserId == userId);

        var allCourseLessons = lesson.Course.Lessons.OrderBy(l => l.LessonOrder).ToList();
        var allCourseLessonIds = allCourseLessons.Select(l => l.LessonId).ToList();

        var completedLessonIds = await _context.LessonProgresses
            .Where(lp => lp.UserId == userId && lp.Completed && allCourseLessonIds.Contains(lp.LessonId))
            .Select(lp => lp.LessonId)
            .ToListAsync();

        var completedCount = completedLessonIds.Count;
        var totalLessons = allCourseLessons.Count;
        var progressPercent = totalLessons > 0 ? (int)Math.Round((double)completedCount / totalLessons * 100) : 0;

        if (enrollment.ProgressPercentage != progressPercent)
        {
            enrollment.ProgressPercentage = progressPercent;
            if (progressPercent >= 100)
            {
                enrollment.Status = "Completed";
            }
            await _context.SaveChangesAsync();
        }

        var prevLesson = allCourseLessons.LastOrDefault(l => l.LessonOrder < lesson.LessonOrder);
        var nextLesson = allCourseLessons.FirstOrDefault(l => l.LessonOrder > lesson.LessonOrder);

        var navList = allCourseLessons.Select(l => new LessonNavViewModel
        {
            LessonId = l.LessonId,
            Title = l.Title,
            LessonOrder = l.LessonOrder,
            Completed = completedLessonIds.Contains(l.LessonId),
            IsCurrent = l.LessonId == lesson.LessonId
        }).ToList();

        var quizzes = await _context.Quizzes
            .Where(q => q.CourseId == lesson.CourseId)
            .Include(q => q.Questions)
            .OrderBy(q => q.QuizId)
            .ToListAsync();

        var viewModel = new LessonViewViewModel
        {
            Lesson = lesson,
            Course = lesson.Course,
            Completed = progress?.Completed == true,
            ProgressPercentage = progressPercent,
            CompletedLessonsCount = completedCount,
            TotalLessonsCount = totalLessons,
            PreviousLesson = prevLesson,
            NextLesson = nextLesson,
            AllCourseLessons = navList,
            CourseQuizzes = quizzes
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int lessonId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var lesson = await _context.Lessons
            .Include(l => l.Course)
                .ThenInclude(c => c.Lessons)
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);

        if (lesson == null)
        {
            return NotFound();
        }

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == lesson.CourseId && e.UserId == userId);

        if (enrollment == null)
        {
            TempData["ErrorMessage"] = "You must be enrolled in this course to mark lessons as completed.";
            return RedirectToAction("Details", "Courses", new { id = lesson.CourseId });
        }

        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.LessonId == lessonId && lp.UserId == userId);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                LessonId = lessonId,
                UserId = userId,
                EnrollmentId = enrollment.EnrollmentId,
                Completed = true,
                CompletedAt = DateTime.UtcNow
            };
            _context.LessonProgresses.Add(progress);
        }
        else if (!progress.Completed)
        {
            progress.Completed = true;
            progress.CompletedAt = DateTime.UtcNow;
            progress.EnrollmentId = enrollment.EnrollmentId;
        }

        var totalLessons = lesson.Course.Lessons.Count;
        var allCourseLessonIds = lesson.Course.Lessons.Select(l => l.LessonId).ToList();

        var completedCount = await _context.LessonProgresses
            .CountAsync(lp => lp.UserId == userId && lp.Completed && allCourseLessonIds.Contains(lp.LessonId));

        if (progress.LessonProgressId == 0) // if newly added
        {
            completedCount++;
        }

        var percent = totalLessons > 0 ? (int)Math.Round((double)completedCount / totalLessons * 100) : 0;
        enrollment.ProgressPercentage = Math.Min(percent, 100);

        if (enrollment.ProgressPercentage >= 100)
        {
            enrollment.Status = "Completed";
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Lesson '{lesson.Title}' marked as complete!";

        var nextIncompleteLesson = lesson.Course.Lessons
            .OrderBy(l => l.LessonOrder)
            .FirstOrDefault(l => l.LessonOrder > lesson.LessonOrder);

        if (nextIncompleteLesson != null)
        {
            return RedirectToAction(nameof(Details), new { id = nextIncompleteLesson.LessonId });
        }

        return RedirectToAction(nameof(Details), new { id = lessonId });
    }
}
