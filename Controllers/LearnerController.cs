using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;
using CampusLearn.ViewModels;

namespace CampusLearn.Controllers;

[Authorize(Roles = "Learner")]
public class LearnerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LearnerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons.OrderBy(l => l.LessonOrder))
            .Include(e => e.Course)
                .ThenInclude(c => c.Quizzes)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync();

        var progresses = await _context.LessonProgresses
            .Where(lp => lp.UserId == userId)
            .ToListAsync();

        var quizResults = await _context.QuizResults
            .Include(qr => qr.Quiz)
                .ThenInclude(q => q.Course)
            .Where(qr => qr.UserId == userId)
            .OrderByDescending(qr => qr.AttemptDate)
            .ToListAsync();

        var completedLessonIds = progresses.Where(p => p.Completed).Select(p => p.LessonId).ToHashSet();
        bool dbStateChanged = false;

        var enrolledViewModels = new List<EnrolledCourseProgressViewModel>();
        foreach (var e in enrollments)
        {
            var totalLessons = e.Course.Lessons.Count;
            var courseLessonIds = e.Course.Lessons.Select(l => l.LessonId).ToList();
            var courseCompletedCount = progresses.Count(p => p.Completed && courseLessonIds.Contains(p.LessonId));
            var progress = totalLessons > 0 ? (int)Math.Round((double)courseCompletedCount / totalLessons * 100) : 0;

            if (e.ProgressPercentage != progress)
            {
                e.ProgressPercentage = progress;
                if (progress >= 100)
                {
                    e.Status = "Completed";
                }
                dbStateChanged = true;
            }

            var nextLesson = e.Course.Lessons
                .OrderBy(l => l.LessonOrder)
                .FirstOrDefault(l => !completedLessonIds.Contains(l.LessonId))
                ?? e.Course.Lessons.OrderBy(l => l.LessonOrder).FirstOrDefault();

            var latestQuizResult = quizResults.FirstOrDefault(qr => qr.Quiz != null && qr.Quiz.CourseId == e.CourseId);
            var availableQuiz = e.Course.Quizzes.FirstOrDefault();

            enrolledViewModels.Add(new EnrolledCourseProgressViewModel
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                CourseDescription = e.Course.Description,
                ImageUrl = e.Course.ImageUrl,
                Category = e.Course.Category,
                Duration = e.Course.Duration,
                EnrollmentDate = e.EnrollmentDate,
                Status = e.Status,
                TotalLessons = totalLessons,
                CompletedLessons = courseCompletedCount,
                ProgressPercentage = progress,
                NextLessonId = nextLesson?.LessonId,
                NextLessonTitle = nextLesson?.Title,
                LatestQuizResult = latestQuizResult,
                AvailableQuizId = availableQuiz?.QuizId,
                AvailableQuizTitle = availableQuiz?.Title
            });
        }

        if (dbStateChanged)
        {
            await _context.SaveChangesAsync();
        }

        var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToList();
        var recommendedCourses = await _context.Courses
            .Include(c => c.Lessons)
            .Where(c => c.IsActive && !enrolledCourseIds.Contains(c.CourseId))
            .OrderByDescending(c => c.CreatedAt)
            .Take(3)
            .ToListAsync();

        var recentQuizViewModels = quizResults.Take(5).Select(qr => new RecentQuizResultViewModel
        {
            QuizResultId = qr.QuizResultId,
            CourseTitle = qr.Quiz.Course.Title,
            QuizTitle = qr.Quiz.Title,
            Score = qr.Score,
            TotalQuestions = qr.TotalQuestions,
            AttemptDate = qr.AttemptDate
        }).ToList();

        var totalQuizzesPassed = quizResults.Count(qr => qr.TotalQuestions > 0 && ((double)qr.Score / qr.TotalQuestions >= 0.6));
        var averageScore = quizResults.Any()
            ? (int)Math.Round(quizResults.Average(qr => qr.TotalQuestions > 0 ? ((double)qr.Score / qr.TotalQuestions * 100) : 0))
            : 0;

        var overallProgress = enrolledViewModels.Any()
            ? (int)Math.Round(enrolledViewModels.Average(e => e.ProgressPercentage))
            : 0;

        var viewModel = new LearnerDashboardViewModel
        {
            User = user,
            TotalEnrolledCourses = enrollments.Count,
            CompletedCoursesCount = enrolledViewModels.Count(e => e.Status == "Completed" || (e.TotalLessons > 0 && e.CompletedLessons == e.TotalLessons)),
            InProgressCoursesCount = enrolledViewModels.Count(e => e.Status != "Completed" && (e.TotalLessons == 0 || e.CompletedLessons < e.TotalLessons)),
            TotalQuizzesPassed = totalQuizzesPassed,
            AverageQuizScore = averageScore,
            OverallProgressPercentage = overallProgress,
            EnrolledCourses = enrolledViewModels,
            RecentQuizResults = recentQuizViewModels,
            RecommendedCourses = recommendedCourses
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> MyCourses()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons.OrderBy(l => l.LessonOrder))
            .Include(e => e.Course)
                .ThenInclude(c => c.Quizzes)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync();

        var progresses = await _context.LessonProgresses
            .Where(lp => lp.UserId == userId)
            .ToListAsync();

        var quizResults = await _context.QuizResults
            .Include(qr => qr.Quiz)
            .Where(qr => qr.UserId == userId)
            .ToListAsync();

        var completedLessonIds = progresses.Where(p => p.Completed).Select(p => p.LessonId).ToHashSet();
        bool dbStateChanged = false;

        var list = new List<EnrolledCourseProgressViewModel>();
        foreach (var e in enrollments)
        {
            var totalLessons = e.Course.Lessons.Count;
            var courseLessonIds = e.Course.Lessons.Select(l => l.LessonId).ToList();
            var courseCompletedCount = progresses.Count(p => p.Completed && courseLessonIds.Contains(p.LessonId));
            var progress = totalLessons > 0 ? (int)Math.Round((double)courseCompletedCount / totalLessons * 100) : 0;

            if (e.ProgressPercentage != progress)
            {
                e.ProgressPercentage = progress;
                if (progress >= 100)
                {
                    e.Status = "Completed";
                }
                dbStateChanged = true;
            }

            var nextLesson = e.Course.Lessons
                .OrderBy(l => l.LessonOrder)
                .FirstOrDefault(l => !completedLessonIds.Contains(l.LessonId))
                ?? e.Course.Lessons.OrderBy(l => l.LessonOrder).FirstOrDefault();

            var latestQuizResult = quizResults.FirstOrDefault(qr => qr.Quiz != null && qr.Quiz.CourseId == e.CourseId);
            var availableQuiz = e.Course.Quizzes.FirstOrDefault();

            list.Add(new EnrolledCourseProgressViewModel
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                CourseDescription = e.Course.Description,
                ImageUrl = e.Course.ImageUrl,
                Category = e.Course.Category,
                Duration = e.Course.Duration,
                EnrollmentDate = e.EnrollmentDate,
                Status = e.Status,
                TotalLessons = totalLessons,
                CompletedLessons = courseCompletedCount,
                ProgressPercentage = progress,
                NextLessonId = nextLesson?.LessonId,
                NextLessonTitle = nextLesson?.Title,
                LatestQuizResult = latestQuizResult,
                AvailableQuizId = availableQuiz?.QuizId,
                AvailableQuizTitle = availableQuiz?.Title
            });
        }

        if (dbStateChanged)
        {
            await _context.SaveChangesAsync();
        }

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Progress()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons.OrderBy(l => l.LessonOrder))
            .Where(e => e.UserId == userId)
            .ToListAsync();

        var progresses = await _context.LessonProgresses
            .Where(lp => lp.UserId == userId)
            .ToListAsync();

        var quizResults = await _context.QuizResults
            .Where(qr => qr.UserId == userId)
            .ToListAsync();

        var courseDetails = new List<CourseProgressDetailViewModel>();

        int totalAllLessons = 0;
        int totalCompletedLessons = progresses.Count(p => p.Completed);

        foreach (var e in enrollments)
        {
            var courseLessons = e.Course.Lessons.OrderBy(l => l.LessonOrder).ToList();
            totalAllLessons += courseLessons.Count;

            var lessonItems = new List<LessonProgressItemViewModel>();
            int courseCompletedLessons = 0;

            foreach (var lesson in courseLessons)
            {
                var prog = progresses.FirstOrDefault(p => p.LessonId == lesson.LessonId);
                var isComp = prog?.Completed == true;
                if (isComp) courseCompletedLessons++;

                lessonItems.Add(new LessonProgressItemViewModel
                {
                    LessonId = lesson.LessonId,
                    LessonTitle = lesson.Title,
                    LessonOrder = lesson.LessonOrder,
                    Completed = isComp,
                    CompletedAt = prog?.CompletedAt
                });
            }

            var courseProgress = courseLessons.Count > 0
                ? (int)Math.Round((double)courseCompletedLessons / courseLessons.Count * 100)
                : 0;

            courseDetails.Add(new CourseProgressDetailViewModel
            {
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                Category = e.Course.Category,
                EnrollmentDate = e.EnrollmentDate,
                Status = e.Status,
                ProgressPercentage = courseProgress,
                Lessons = lessonItems
            });
        }

        var overallProgress = totalAllLessons > 0
            ? (int)Math.Round((double)totalCompletedLessons / totalAllLessons * 100)
            : 0;

        var avgScore = quizResults.Any()
            ? Math.Round(quizResults.Average(qr => qr.TotalQuestions > 0 ? ((double)qr.Score / qr.TotalQuestions * 100) : 0), 1)
            : 0.0;

        var viewModel = new LearnerProgressViewModel
        {
            User = user,
            OverallProgressPercentage = overallProgress,
            TotalLessonsCompleted = totalCompletedLessons,
            TotalQuizzesTaken = quizResults.Count,
            OverallAverageScore = avgScore,
            CourseDetails = courseDetails
        };

        return View(viewModel);
    }
}
