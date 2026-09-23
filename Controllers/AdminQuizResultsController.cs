using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;

namespace CampusLearn.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminQuizResultsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminQuizResultsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? userId, int? courseId, int? quizId)
    {
        var query = _context.QuizResults
            .Include(qr => qr.User)
            .Include(qr => qr.Quiz)
                .ThenInclude(q => q.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(qr => qr.UserId == userId);
            ViewBag.SelectedLearner = await _userManager.FindByIdAsync(userId);
        }

        if (courseId.HasValue && courseId.Value > 0)
        {
            query = query.Where(qr => qr.Quiz.CourseId == courseId.Value);
            ViewBag.SelectedCourse = await _context.Courses.FindAsync(courseId.Value);
        }

        if (quizId.HasValue && quizId.Value > 0)
        {
            query = query.Where(qr => qr.QuizId == quizId.Value);
            ViewBag.SelectedQuiz = await _context.Quizzes.FindAsync(quizId.Value);
        }

        var results = await query.OrderByDescending(qr => qr.AttemptDate).ToListAsync();

        var courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
        var quizzes = await _context.Quizzes.OrderBy(q => q.Title).ToListAsync();
        var learners = await _userManager.GetUsersInRoleAsync("Learner");

        ViewBag.Courses = courses;
        ViewBag.Quizzes = quizzes;
        ViewBag.Learners = learners.OrderBy(l => l.FullName ?? l.UserName).ToList();
        ViewBag.SelectedUserId = userId;
        ViewBag.SelectedCourseId = courseId;
        ViewBag.SelectedQuizId = quizId;

        return View(results);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _context.QuizResults
            .Include(qr => qr.User)
            .Include(qr => qr.Quiz)
                .ThenInclude(q => q.Questions)
            .Include(qr => qr.Quiz)
                .ThenInclude(q => q.Course)
            .FirstOrDefaultAsync(qr => qr.QuizResultId == id);

        if (result == null)
        {
            return NotFound();
        }

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _context.QuizResults
            .Include(qr => qr.User)
            .Include(qr => qr.Quiz)
                .ThenInclude(q => q.Course)
            .FirstOrDefaultAsync(qr => qr.QuizResultId == id);

        if (result == null)
        {
            return NotFound();
        }

        return View(result);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _context.QuizResults.FindAsync(id);
        if (result != null)
        {
            _context.QuizResults.Remove(result);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Quiz result submission record was deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
