using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;

namespace CampusLearn.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var featuredCourses = await _context.Courses
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .OrderByDescending(c => c.CreatedAt)
            .Take(6)
            .ToListAsync();

        ViewBag.TotalCourses = await _context.Courses.CountAsync();
        ViewBag.TotalLearners = await _context.Users.CountAsync();
        ViewBag.TotalEnrollments = await _context.Enrollments.CountAsync();
        ViewBag.FeaturedCourses = featuredCourses;

        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Contact()
    {
        return View(new ContactMessage());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactMessage model)
    {
        if (ModelState.IsValid)
        {
            model.SubmittedAt = DateTime.UtcNow;
            _context.ContactMessages.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you for contacting CampusLearn! Your message has been received and our team will get back to you soon.";
            return RedirectToAction(nameof(Contact));
        }

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
