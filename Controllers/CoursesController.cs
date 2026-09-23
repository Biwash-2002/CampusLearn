using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;
using CampusLearn.ViewModels;

namespace CampusLearn.Controllers;

public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? category)
    {
        var query = _context.Courses
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .Where(c => c.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(c => c.Title.ToLower().Contains(s.ToLower()) || (c.Description != null && c.Description.ToLower().Contains(s.ToLower())));
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            query = query.Where(c => c.Category == category);
        }

        var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

        var allCategories = await _context.Courses
            .Where(c => c.Category != null)
            .Select(c => c.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var viewModel = new CourseListViewModel
        {
            Courses = courses,
            SearchTerm = search,
            SelectedCategory = category,
            Categories = allCategories,
            TotalCoursesCount = courses.Count
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Lessons.OrderBy(l => l.LessonOrder))
            .Include(c => c.Quizzes)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }

        var viewModel = new CourseDetailViewModel
        {
            Course = course,
            IsEnrolled = false,
            CompletedLessonsCount = 0,
            ProgressPercentage = 0
        };

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == id && e.UserId == userId);

            if (enrollment != null)
            {
                viewModel.IsEnrolled = true;

                var completedLessonIds = await _context.LessonProgresses
                    .Where(lp => lp.UserId == userId && lp.Completed && course.Lessons.Select(l => l.LessonId).Contains(lp.LessonId))
                    .Select(lp => lp.LessonId)
                    .ToListAsync();

                viewModel.CompletedLessonsCount = completedLessonIds.Count;
                var totalLessons = course.Lessons.Count;
                viewModel.ProgressPercentage = totalLessons > 0 ? (int)Math.Round((double)completedLessonIds.Count / totalLessons * 100) : 0;

                var nextLesson = course.Lessons
                    .OrderBy(l => l.LessonOrder)
                    .FirstOrDefault(l => !completedLessonIds.Contains(l.LessonId)) ?? course.Lessons.OrderBy(l => l.LessonOrder).FirstOrDefault();

                viewModel.NextLessonId = nextLesson?.LessonId;

                viewModel.Lessons = course.Lessons.OrderBy(l => l.LessonOrder).Select(l => new LessonStatusViewModel
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    LessonOrder = l.LessonOrder,
                    IsCompleted = completedLessonIds.Contains(l.LessonId)
                }).ToList();
            }
        }

        if (!viewModel.IsEnrolled)
        {
            viewModel.Lessons = course.Lessons.OrderBy(l => l.LessonOrder).Select(l => new LessonStatusViewModel
            {
                LessonId = l.LessonId,
                Title = l.Title,
                LessonOrder = l.LessonOrder,
                IsCompleted = false
            }).ToList();
        }

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Learner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId && c.IsActive);
        if (course == null)
        {
            return NotFound();
        }

        var existingEnrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId);

        if (existingEnrollment == null)
        {
            var enrollment = new Enrollment
            {
                CourseId = courseId,
                UserId = userId,
                EnrollmentDate = DateTime.UtcNow,
                ProgressPercentage = 0,
                Status = "Active"
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"You have successfully enrolled in '{course.Title}'!";
        }
        else
        {
            TempData["InfoMessage"] = $"You are already enrolled in '{course.Title}'.";
        }

        return RedirectToAction("MyCourses", "Learner");
    }
}
