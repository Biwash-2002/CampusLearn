using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;
using CampusLearn.ViewModels;

namespace CampusLearn.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminLearnersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminLearnersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var learners = await _userManager.GetUsersInRoleAsync("Learner");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            learners = learners.Where(l =>
                (!string.IsNullOrEmpty(l.FullName) && l.FullName.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(l.Email) && l.Email.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(l.UserName) && l.UserName.Contains(s, StringComparison.OrdinalIgnoreCase))
            ).ToList();
            ViewBag.Search = search;
        }

        var learnerIds = learners.Select(l => l.Id).ToList();

        var enrollments = await _context.Enrollments
            .Where(e => learnerIds.Contains(e.UserId))
            .ToListAsync();

        var quizResults = await _context.QuizResults
            .Where(qr => learnerIds.Contains(qr.UserId))
            .ToListAsync();

        var viewModels = learners.Select(l => new LearnerItemViewModel
        {
            Id = l.Id,
            FullName = !string.IsNullOrWhiteSpace(l.FullName) ? l.FullName : (l.UserName ?? "Learner"),
            Email = l.Email ?? "",
            CreatedAt = l.CreatedAt,
            EnrolledCoursesCount = enrollments.Count(e => e.UserId == l.Id),
            CompletedCoursesCount = enrollments.Count(e => e.UserId == l.Id && e.Status == "Completed"),
            QuizzesTakenCount = quizResults.Count(qr => qr.UserId == l.Id)
        }).OrderByDescending(l => l.CreatedAt).ToList();

        return View(viewModels);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var learner = await _userManager.FindByIdAsync(id);
        if (learner == null)
        {
            return NotFound();
        }

        var isLearner = await _userManager.IsInRoleAsync(learner, "Learner");
        if (!isLearner)
        {
            return NotFound();
        }

        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .Where(e => e.UserId == id)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync();

        var lessonProgresses = await _context.LessonProgresses
            .Include(lp => lp.Lesson)
                .ThenInclude(l => l.Course)
            .Where(lp => lp.UserId == id)
            .OrderByDescending(lp => lp.CompletedAt)
            .ToListAsync();

        var quizResults = await _context.QuizResults
            .Include(qr => qr.Quiz)
                .ThenInclude(q => q.Course)
            .Where(qr => qr.UserId == id)
            .OrderByDescending(qr => qr.AttemptDate)
            .ToListAsync();

        var model = new LearnerDetailViewModel
        {
            Learner = learner,
            Enrollments = enrollments,
            LessonProgresses = lessonProgresses,
            QuizResults = quizResults
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var learner = await _userManager.FindByIdAsync(id);
        if (learner == null)
        {
            return NotFound();
        }

        var isLearner = await _userManager.IsInRoleAsync(learner, "Learner");
        if (!isLearner)
        {
            return NotFound();
        }

        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (learner.Id == currentAdminId)
        {
            TempData["ErrorMessage"] = "You cannot delete your own administrator account.";
            return RedirectToAction(nameof(Index));
        }

        var enrolledCount = await _context.Enrollments.CountAsync(e => e.UserId == id);
        var completedLessonsCount = await _context.LessonProgresses.CountAsync(lp => lp.UserId == id && lp.Completed);
        var quizResultsCount = await _context.QuizResults.CountAsync(qr => qr.UserId == id);

        var model = new LearnerDeleteViewModel
        {
            Id = learner.Id,
            FullName = !string.IsNullOrWhiteSpace(learner.FullName) ? learner.FullName : (learner.UserName ?? "Learner"),
            Email = learner.Email ?? "",
            CreatedAt = learner.CreatedAt,
            EnrolledCoursesCount = enrolledCount,
            CompletedLessonsCount = completedLessonsCount,
            QuizResultsCount = quizResultsCount
        };

        return View(model);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var learner = await _userManager.FindByIdAsync(id);
        if (learner == null)
        {
            return NotFound();
        }

        var isLearner = await _userManager.IsInRoleAsync(learner, "Learner");
        if (!isLearner)
        {
            TempData["ErrorMessage"] = "Target account is not a learner account.";
            return RedirectToAction(nameof(Index));
        }

        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (learner.Id == currentAdminId)
        {
            TempData["ErrorMessage"] = "You cannot delete your own administrator account.";
            return RedirectToAction(nameof(Index));
        }

        var name = learner.FullName ?? learner.Email ?? "Learner";
        var result = await _userManager.DeleteAsync(learner);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"Learner account '{name}' and all associated records were deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete the learner account: " + string.Join(", ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Index));
    }
}
