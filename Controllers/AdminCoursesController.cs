using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;
using CampusLearn.ViewModels;

namespace CampusLearn.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminCoursesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminCoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? category)
    {
        var query = _context.Courses
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(s) || c.Description.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            query = query.Where(c => c.Category == category);
        }

        var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

        var allCategories = await _context.Courses
            .Select(c => c.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        ViewBag.SearchTerm = search;
        ViewBag.SelectedCategory = category;
        ViewBag.Categories = allCategories;

        return View(courses);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Lessons.OrderBy(l => l.LessonOrder))
            .Include(c => c.Quizzes)
                .ThenInclude(q => q.Questions)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CourseFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            var course = new Course
            {
                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                Category = model.Category.Trim(),
                Duration = model.Duration.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? "/images/default-course.jpg" : model.ImageUrl.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Course '{course.Title}' created successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        var model = new CourseFormViewModel
        {
            CourseId = course.CourseId,
            Title = course.Title,
            Description = course.Description,
            Category = course.Category,
            Duration = course.Duration,
            ImageUrl = course.ImageUrl,
            IsActive = course.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseFormViewModel model)
    {
        if (id != model.CourseId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            course.Title = model.Title.Trim();
            course.Description = model.Description.Trim();
            course.Category = model.Category.Trim();
            course.Duration = model.Duration.Trim();
            course.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim();
            course.IsActive = model.IsActive;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Course '{course.Title}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            var title = course.Title;
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Course '{title}' deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
