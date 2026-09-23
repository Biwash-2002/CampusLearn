using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;
using CampusLearn.ViewModels;

namespace CampusLearn.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminQuizzesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminQuizzesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? courseId)
    {
        var query = _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions)
            .Include(q => q.QuizResults)
            .AsQueryable();

        if (courseId.HasValue)
        {
            query = query.Where(q => q.CourseId == courseId.Value);
            ViewBag.SelectedCourse = await _context.Courses.FindAsync(courseId.Value);
        }

        var quizzes = await query.OrderBy(q => q.Course.Title).ThenBy(q => q.Title).ToListAsync();
        ViewBag.Courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
        ViewBag.CourseId = courseId;

        return View(quizzes);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions)
            .Include(q => q.QuizResults)
                .ThenInclude(qr => qr.User)
            .FirstOrDefaultAsync(q => q.QuizId == id);

        if (quiz == null)
        {
            return NotFound();
        }

        return View(quiz);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? courseId)
    {
        var courses = await _context.Courses
            .OrderBy(c => c.Title)
            .ToListAsync();

        ViewBag.CoursesList = new SelectList(courses, "CourseId", "Title", courseId);

        var model = new QuizFormViewModel
        {
            CourseId = courseId ?? (courses.FirstOrDefault()?.CourseId ?? 0),
            Questions = new List<QuizQuestionFormViewModel>
            {
                new() { QuestionText = "", OptionA = "", OptionB = "", OptionC = "", OptionD = "", CorrectAnswer = "A" }
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuizFormViewModel model)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == model.CourseId);
        if (!courseExists)
        {
            ModelState.AddModelError(nameof(model.CourseId), "Selected course does not exist.");
        }

        if (model.Questions == null || !model.Questions.Any())
        {
            ModelState.AddModelError(string.Empty, "At least one question is required for a quiz.");
        }

        if (ModelState.IsValid)
        {
            var quiz = new Quiz
            {
                CourseId = model.CourseId,
                Title = model.Title,
                Description = model.Description,
                Questions = model.Questions!.Select(q => new Question
                {
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    CorrectAnswer = q.CorrectAnswer.Trim().ToUpperInvariant()
                }).ToList()
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Quiz '{quiz.Title}' with {quiz.Questions.Count} question(s) created successfully!";
            return RedirectToAction(nameof(Details), new { id = quiz.QuizId });
        }

        var courses = await _context.Courses
            .OrderBy(c => c.Title)
            .ToListAsync();

        ViewBag.CoursesList = new SelectList(courses, "CourseId", "Title", model.CourseId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.QuizId == id);

        if (quiz == null)
        {
            return NotFound();
        }

        var courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
        ViewBag.CoursesList = new SelectList(courses, "CourseId", "Title", quiz.CourseId);

        var model = new QuizFormViewModel
        {
            QuizId = quiz.QuizId,
            CourseId = quiz.CourseId,
            Title = quiz.Title,
            Description = quiz.Description,
            CourseTitle = quiz.Course?.Title,
            Questions = quiz.Questions.Select(q => new QuizQuestionFormViewModel
            {
                QuestionId = q.QuestionId,
                QuizId = q.QuizId,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectAnswer
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, QuizFormViewModel model)
    {
        if (id != model.QuizId)
        {
            return NotFound();
        }

        var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == model.CourseId);
        if (!courseExists)
        {
            ModelState.AddModelError(nameof(model.CourseId), "Selected course does not exist.");
        }

        if (model.Questions == null || !model.Questions.Any())
        {
            ModelState.AddModelError(string.Empty, "At least one question is required for a quiz.");
        }

        if (ModelState.IsValid)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
            {
                return NotFound();
            }

            quiz.CourseId = model.CourseId;
            quiz.Title = model.Title;
            quiz.Description = model.Description;

            _context.Questions.RemoveRange(quiz.Questions);

            quiz.Questions = model.Questions!.Select(q => new Question
            {
                QuizId = quiz.QuizId,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectAnswer.Trim().ToUpperInvariant()
            }).ToList();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Quiz '{quiz.Title}' updated successfully!";
            return RedirectToAction(nameof(Details), new { id = quiz.QuizId });
        }

        var courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
        ViewBag.CoursesList = new SelectList(courses, "CourseId", "Title", model.CourseId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions)
            .Include(q => q.QuizResults)
            .FirstOrDefaultAsync(q => q.QuizId == id);

        if (quiz == null)
        {
            return NotFound();
        }

        return View(quiz);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz != null)
        {
            var title = quiz.Title;
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Quiz '{title}' and all associated questions and results were removed successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Question Management Actions
    [HttpGet]
    public async Task<IActionResult> AddQuestion(int quizId)
    {
        var quiz = await _context.Quizzes.Include(q => q.Course).FirstOrDefaultAsync(q => q.QuizId == quizId);
        if (quiz == null)
        {
            return NotFound();
        }

        var model = new QuestionFormViewModel
        {
            QuizId = quizId,
            QuizTitle = quiz.Title,
            CourseTitle = quiz.Course.Title,
            CorrectAnswer = "A"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(QuestionFormViewModel model)
    {
        var quiz = await _context.Quizzes.Include(q => q.Course).FirstOrDefaultAsync(q => q.QuizId == model.QuizId);
        if (quiz == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var question = new Question
            {
                QuizId = model.QuizId,
                QuestionText = model.QuestionText,
                OptionA = model.OptionA,
                OptionB = model.OptionB,
                OptionC = model.OptionC,
                OptionD = model.OptionD,
                CorrectAnswer = model.CorrectAnswer.Trim().ToUpperInvariant()
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Question added to quiz successfully!";
            return RedirectToAction(nameof(Details), new { id = model.QuizId });
        }

        model.QuizTitle = quiz.Title;
        model.CourseTitle = quiz.Course.Title;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditQuestion(int id)
    {
        var question = await _context.Questions
            .Include(q => q.Quiz)
                .ThenInclude(qz => qz.Course)
            .FirstOrDefaultAsync(q => q.QuestionId == id);

        if (question == null)
        {
            return NotFound();
        }

        var model = new QuestionFormViewModel
        {
            QuestionId = question.QuestionId,
            QuizId = question.QuizId,
            QuizTitle = question.Quiz.Title,
            CourseTitle = question.Quiz.Course.Title,
            QuestionText = question.QuestionText,
            OptionA = question.OptionA,
            OptionB = question.OptionB,
            OptionC = question.OptionC,
            OptionD = question.OptionD,
            CorrectAnswer = question.CorrectAnswer
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(int id, QuestionFormViewModel model)
    {
        if (id != model.QuestionId)
        {
            return NotFound();
        }

        var question = await _context.Questions
            .Include(q => q.Quiz)
                .ThenInclude(qz => qz.Course)
            .FirstOrDefaultAsync(q => q.QuestionId == id);

        if (question == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            question.QuestionText = model.QuestionText;
            question.OptionA = model.OptionA;
            question.OptionB = model.OptionB;
            question.OptionC = model.OptionC;
            question.OptionD = model.OptionD;
            question.CorrectAnswer = model.CorrectAnswer.Trim().ToUpperInvariant();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Question updated successfully!";
            return RedirectToAction(nameof(Details), new { id = question.QuizId });
        }

        model.QuizTitle = question.Quiz.Title;
        model.CourseTitle = question.Quiz.Course.Title;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await _context.Questions
            .Include(q => q.Quiz)
                .ThenInclude(qz => qz.Course)
            .FirstOrDefaultAsync(q => q.QuestionId == id);

        if (question == null)
        {
            return NotFound();
        }

        return View(question);
    }

    [HttpPost, ActionName("DeleteQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestionConfirmed(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question != null)
        {
            var quizId = question.QuizId;
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Question removed from quiz successfully.";
            return RedirectToAction(nameof(Details), new { id = quizId });
        }

        return RedirectToAction(nameof(Index));
    }
}
