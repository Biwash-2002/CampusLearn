using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusLearn.Data;
using CampusLearn.Models;
using CampusLearn.ViewModels;

namespace CampusLearn.Controllers;

[Authorize(Roles = "Learner")]
public class QuizzesController : Controller
{
    private readonly ApplicationDbContext _context;

    public QuizzesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Take(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.QuizId == id);

        if (quiz == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.CourseId == quiz.CourseId && e.UserId == userId);

        if (!isEnrolled)
        {
            TempData["ErrorMessage"] = "You must be enrolled in the course to take this quiz.";
            return RedirectToAction("Details", "Courses", new { id = quiz.CourseId });
        }

        var questionsViewModel = quiz.Questions
            .OrderBy(q => q.QuestionId)
            .Select(q => new QuizQuestionAnswerViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                SelectedOption = null
            }).ToList();

        var viewModel = new QuizTakingViewModel
        {
            QuizId = quiz.QuizId,
            QuizTitle = quiz.Title,
            QuizDescription = quiz.Description,
            CourseId = quiz.CourseId,
            CourseTitle = quiz.Course.Title,
            Questions = questionsViewModel
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int quizId, IFormCollection form)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .Include(q => q.Course)
            .FirstOrDefaultAsync(q => q.QuizId == quizId);

        if (quiz == null)
        {
            return NotFound();
        }

        var isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.CourseId == quiz.CourseId && e.UserId == userId);

        if (!isEnrolled)
        {
            TempData["ErrorMessage"] = "You must be enrolled in the course to submit quiz answers.";
            return RedirectToAction("Details", "Courses", new { id = quiz.CourseId });
        }

        int score = 0;
        int total = quiz.Questions.Count;

        foreach (var question in quiz.Questions)
        {
            var formKey = $"question_{question.QuestionId}";
            var selectedOption = form[formKey].ToString()?.Trim().ToUpperInvariant();

            if (!string.IsNullOrEmpty(selectedOption) &&
                string.Equals(selectedOption, question.CorrectAnswer.Trim().ToUpperInvariant(), StringComparison.OrdinalIgnoreCase))
            {
                score++;
            }
        }

        var quizResult = new QuizResult
        {
            QuizId = quizId,
            UserId = userId,
            Score = score,
            TotalQuestions = total,
            AttemptDate = DateTime.UtcNow
        };

        _context.QuizResults.Add(quizResult);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Quiz submitted successfully!";
        return RedirectToAction(nameof(Result), new { id = quizResult.QuizResultId });
    }

    [HttpGet]
    public async Task<IActionResult> Result(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var result = await _context.QuizResults
            .Include(qr => qr.Quiz)
                .ThenInclude(q => q.Questions)
            .Include(qr => qr.Quiz)
                .ThenInclude(q => q.Course)
            .FirstOrDefaultAsync(qr => qr.QuizResultId == id);

        if (result == null)
        {
            return NotFound();
        }

        if (result.UserId != userId)
        {
            return Forbid();
        }

        var feedbacks = result.Quiz.Questions
            .OrderBy(q => q.QuestionId)
            .Select(q => new QuizQuestionFeedbackViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectAnswer,
                UserAnswer = null
            }).ToList();

        var viewModel = new QuizResultDetailViewModel
        {
            Result = result,
            QuizId = result.Quiz.QuizId,
            QuizTitle = result.Quiz.Title,
            QuizDescription = result.Quiz.Description,
            CourseId = result.Quiz.Course.CourseId,
            CourseTitle = result.Quiz.Course.Title,
            QuestionFeedbacks = feedbacks
        };

        return View(viewModel);
    }
}
