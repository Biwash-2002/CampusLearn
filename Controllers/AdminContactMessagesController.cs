using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;

namespace CampusLearn.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminContactMessagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminContactMessagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var messages = await _context.ContactMessages
            .OrderByDescending(m => m.SubmittedAt)
            .ToListAsync();

        return View(messages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var msg = await _context.ContactMessages.FindAsync(id);
        if (msg != null)
        {
            _context.ContactMessages.Remove(msg);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Contact message deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
