using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;
using CampusLearn.ViewModels;

namespace CampusLearn.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminLessonsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminLessonsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? courseId)
    {
        var query = _context.Lessons
            .Include(l => l.Course)
            .Include(l => l.LessonProgresses)
            .AsQueryable();

        if (courseId.HasValue)
        {
            query = query.Where(l => l.CourseId == courseId.Value);
            var currentCourse = await _context.Courses.FindAsync(courseId.Value);
            ViewBag.SelectedCourse = currentCourse;
        }

        var lessons = await query.OrderBy(l => l.Course.Title).ThenBy(l => l.LessonOrder).ToListAsync();
        ViewBag.Courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
        ViewBag.CourseId = courseId;

        return View(lessons);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Course)
            .Include(l => l.LessonProgresses)
                .ThenInclude(lp => lp.User)
            .FirstOrDefaultAsync(l => l.LessonId == id);

        if (lesson == null)
        {
            return NotFound();
        }

        return View(lesson);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? courseId)
    {
        var courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
        ViewBag.CoursesList = new SelectList(courses, "CourseId", "Title", courseId);

        int nextOrder = 1;
        if (courseId.HasValue)
        {
            var maxOrder = await _context.Lessons
                .Where(l => l.CourseId == courseId.Value)
                .Select(l => (int?)l.LessonOrder)
                .MaxAsync();
            nextOrder = (maxOrder ?? 0) + 1;
        }

        var model = new LessonFormViewModel
        {
            CourseId = courseId ?? (courses.FirstOrDefault()?.CourseId ?? 0),
            LessonOrder = nextOrder
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonFormViewModel model)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == model.CourseId);
        if (!courseExists)
        {
            ModelState.AddModelError(nameof(model.CourseId), "Selected course does not exist.");
        }

        if (ModelState.IsValid)
        {
            var lesson = new Lesson
            {
                CourseId = model.CourseId,
                Title = model.Title,
                Content = model.Content,
                LessonOrder = model.LessonOrder,
                CreatedAt = DateTime.UtcNow
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Lesson '{lesson.Title}' added successfully!";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        var courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
        ViewBag.CoursesList = new SelectList(courses, "CourseId", "Title", model.CourseId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var lesson = await _context.Lessons.Include(l => l.Course).FirstOrDefaultAsync(l => l.LessonId == id);
        if (lesson == null)
        {
            return NotFound();
        }

        var courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
        ViewBag.CoursesList = new SelectList(courses, "CourseId", "Title", lesson.CourseId);

        var model = new LessonFormViewModel
        {
            LessonId = lesson.LessonId,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Content = lesson.Content,
            LessonOrder = lesson.LessonOrder,
            CourseTitle = lesson.Course?.Title
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LessonFormViewModel model)
    {
        if (id != model.LessonId)
        {
            return NotFound();
        }

        var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == model.CourseId);
        if (!courseExists)
        {
            ModelState.AddModelError(nameof(model.CourseId), "Selected course does not exist.");
        }

        if (ModelState.IsValid)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            lesson.CourseId = model.CourseId;
            lesson.Title = model.Title;
            lesson.Content = model.Content;
            lesson.LessonOrder = model.LessonOrder;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Lesson '{lesson.Title}' updated successfully!";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        var courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
        ViewBag.CoursesList = new SelectList(courses, "CourseId", "Title", model.CourseId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.LessonId == id);

        if (lesson == null)
        {
            return NotFound();
        }

        return View(lesson);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson != null)
        {
            var courseId = lesson.CourseId;
            var title = lesson.Title;
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Lesson '{title}' deleted successfully.";
            return RedirectToAction(nameof(Index), new { courseId });
        }

        return RedirectToAction(nameof(Index));
    }
}
