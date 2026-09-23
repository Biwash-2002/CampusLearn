using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;
using CampusLearn.ViewModels;

namespace CampusLearn.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminEnrollmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminEnrollmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? courseId, string? userId)
    {
        var query = _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .AsQueryable();

        if (courseId.HasValue && courseId.Value > 0)
        {
            query = query.Where(e => e.CourseId == courseId.Value);
            ViewBag.SelectedCourse = await _context.Courses.FindAsync(courseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(e => e.UserId == userId);
            ViewBag.SelectedLearner = await _userManager.FindByIdAsync(userId);
        }

        var enrollments = await query.OrderByDescending(e => e.EnrollmentDate).ToListAsync();

        var courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
        var learners = await _userManager.GetUsersInRoleAsync("Learner");

        ViewBag.Courses = courses;
        ViewBag.Learners = learners.OrderBy(l => l.FullName ?? l.UserName).ToList();
        ViewBag.CourseId = courseId;
        ViewBag.UserId = userId;

        ViewBag.CoursesList = new SelectList(courses, "CourseId", "Title", courseId);
        ViewBag.LearnersList = new SelectList(learners.OrderBy(l => l.FullName ?? l.Email), "Id", "Email", userId);

        return View(enrollments);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons.OrderBy(l => l.LessonOrder))
            .Include(e => e.Course)
                .ThenInclude(c => c.Quizzes)
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);

        if (enrollment == null)
        {
            return NotFound();
        }

        var courseLessonIds = enrollment.Course.Lessons.Select(l => l.LessonId).ToList();

        var lessonProgresses = await _context.LessonProgresses
            .Include(lp => lp.Lesson)
            .Where(lp => lp.UserId == enrollment.UserId && courseLessonIds.Contains(lp.LessonId))
            .OrderBy(lp => lp.Lesson.LessonOrder)
            .ToListAsync();

        var quizResult = await _context.QuizResults
            .Include(qr => qr.Quiz)
            .FirstOrDefaultAsync(qr => qr.UserId == enrollment.UserId && qr.Quiz.CourseId == enrollment.CourseId);

        var totalLessons = enrollment.Course.Lessons.Count;
        var completedLessons = lessonProgresses.Count(lp => lp.Completed);

        var model = new EnrollmentDetailViewModel
        {
            Enrollment = enrollment,
            LessonProgresses = lessonProgresses,
            QuizResult = quizResult,
            CompletedLessonsCount = completedLessons,
            TotalLessonsCount = totalLessons
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string userId, int courseId)
    {
        if (string.IsNullOrEmpty(userId) || courseId <= 0)
        {
            TempData["ErrorMessage"] = "Please select both a learner and a course.";
            return RedirectToAction(nameof(Index));
        }

        var learner = await _userManager.FindByIdAsync(userId);
        var course = await _context.Courses.FindAsync(courseId);

        if (learner == null || course == null)
        {
            TempData["ErrorMessage"] = "The specified learner or course does not exist.";
            return RedirectToAction(nameof(Index));
        }

        var exists = await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
        if (exists)
        {
            TempData["ErrorMessage"] = $"Learner '{learner.FullName ?? learner.Email}' is already enrolled in '{course.Title}'.";
            return RedirectToAction(nameof(Index));
        }

        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow,
            ProgressPercentage = 0,
            Status = "Active"
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Learner '{learner.FullName ?? learner.Email}' enrolled into '{course.Title}' successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);

        if (enrollment == null)
        {
            return NotFound();
        }

        var courseLessonIds = enrollment.Course?.Lessons?.Select(l => l.LessonId).ToList() ?? new List<int>();

        var lessonProgressCount = await _context.LessonProgresses
            .CountAsync(lp => lp.EnrollmentId == id || (lp.UserId == enrollment.UserId && courseLessonIds.Contains(lp.LessonId)));

        ViewBag.LessonProgressCount = lessonProgressCount;

        return View(enrollment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);

        if (enrollment != null)
        {
            var learnerName = enrollment.User?.FullName ?? enrollment.User?.Email ?? "Learner";
            var courseTitle = enrollment.Course?.Title ?? "Course";

            // Remove associated lesson progresses for this course & learner
            var courseLessonIds = enrollment.Course?.Lessons?.Select(l => l.LessonId).ToList() ?? new List<int>();
            var progresses = await _context.LessonProgresses
                .Where(lp => lp.EnrollmentId == id || (lp.UserId == enrollment.UserId && courseLessonIds.Contains(lp.LessonId)))
                .ToListAsync();

            if (progresses.Any())
            {
                _context.LessonProgresses.RemoveRange(progresses);
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Enrollment of '{learnerName}' in '{courseTitle}' was removed successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
