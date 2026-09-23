using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;
using CampusLearn.ViewModels;

namespace CampusLearn.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var totalCourses = await _context.Courses.CountAsync();
        var totalLessons = await _context.Lessons.CountAsync();
        var totalQuizzes = await _context.Quizzes.CountAsync();
        var learnersList = await _userManager.GetUsersInRoleAsync("Learner");
        var totalLearners = learnersList.Count;
        var totalEnrollments = await _context.Enrollments.CountAsync();
        var totalQuizAttempts = await _context.QuizResults.CountAsync();
        var totalContactMessages = await _context.ContactMessages.CountAsync();

        var recentCourses = await _context.Courses
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentEnrollments = await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .OrderByDescending(e => e.EnrollmentDate)
            .Take(5)
            .ToListAsync();

        var recentQuizResults = await _context.QuizResults
            .Include(qr => qr.User)
            .Include(qr => qr.Quiz)
            .OrderByDescending(qr => qr.AttemptDate)
            .Take(5)
            .ToListAsync();

        var recentMessages = await _context.ContactMessages
            .OrderByDescending(m => m.SubmittedAt)
            .Take(5)
            .ToListAsync();

        var viewModel = new AdminDashboardViewModel
        {
            TotalCourses = totalCourses,
            TotalLessons = totalLessons,
            TotalQuizzes = totalQuizzes,
            TotalLearners = totalLearners,
            TotalEnrollments = totalEnrollments,
            TotalQuizAttempts = totalQuizAttempts,
            TotalContactMessages = totalContactMessages,
            RecentCourses = recentCourses,
            RecentEnrollments = recentEnrollments,
            RecentQuizResults = recentQuizResults,
            RecentMessages = recentMessages
        };

        return View(viewModel);
    }
}
