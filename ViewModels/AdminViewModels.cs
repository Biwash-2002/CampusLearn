using System.ComponentModel.DataAnnotations;
using CampusLearn.Models;

namespace CampusLearn.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalCourses { get; set; }
    public int TotalLessons { get; set; }
    public int TotalQuizzes { get; set; }
    public int TotalLearners { get; set; }
    public int TotalEnrollments { get; set; }
    public int TotalQuizAttempts { get; set; }
    public int TotalContactMessages { get; set; }
    public List<Course> RecentCourses { get; set; } = new();
    public List<Enrollment> RecentEnrollments { get; set; } = new();
    public List<QuizResult> RecentQuizResults { get; set; } = new();
    public List<ContactMessage> RecentMessages { get; set; } = new();
}

public class CourseFormViewModel
{
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Duration is required (e.g. '6 Weeks' or '20 Hours')")]
    [StringLength(50, ErrorMessage = "Duration cannot exceed 50 characters")]
    [Display(Name = "Duration")]
    public string Duration { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
    [Display(Name = "Image URL (Optional)")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}

public class LessonFormViewModel
{
    public int LessonId { get; set; }

    [Required(ErrorMessage = "Course is required")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [Display(Name = "Lesson Content")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lesson Order is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Lesson order must be a positive number (1 or greater)")]
    [Display(Name = "Lesson Order")]
    public int LessonOrder { get; set; } = 1;

    public string? CourseTitle { get; set; }
}

public class QuizFormViewModel
{
    public int QuizId { get; set; }

    [Required(ErrorMessage = "Course is required")]
    [Display(Name = "Associated Course")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Quiz Title is required")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Quiz Description")]
    public string? Description { get; set; }

    public string? CourseTitle { get; set; }

    public List<QuizQuestionFormViewModel> Questions { get; set; } = new();
}

public class QuizQuestionFormViewModel
{
    public int QuestionId { get; set; }
    public int QuizId { get; set; }

    [Required(ErrorMessage = "Question text is required")]
    [Display(Name = "Question Text")]
    public string QuestionText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option A is required")]
    [Display(Name = "Option A")]
    public string OptionA { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option B is required")]
    [Display(Name = "Option B")]
    public string OptionB { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option C is required")]
    [Display(Name = "Option C")]
    public string OptionC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option D is required")]
    [Display(Name = "Option D")]
    public string OptionD { get; set; } = string.Empty;

    [Required(ErrorMessage = "Correct answer is required (A, B, C, or D)")]
    [RegularExpression("^[A-Da-d]$", ErrorMessage = "Must be A, B, C, or D")]
    [Display(Name = "Correct Answer (A/B/C/D)")]
    public string CorrectAnswer { get; set; } = "A";
}

public class QuestionFormViewModel
{
    public int QuestionId { get; set; }

    [Required(ErrorMessage = "Quiz is required")]
    [Display(Name = "Quiz")]
    public int QuizId { get; set; }

    public string? QuizTitle { get; set; }
    public string? CourseTitle { get; set; }

    [Required(ErrorMessage = "Question text is required")]
    [Display(Name = "Question Text")]
    public string QuestionText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option A is required")]
    [StringLength(500)]
    [Display(Name = "Option A")]
    public string OptionA { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option B is required")]
    [StringLength(500)]
    [Display(Name = "Option B")]
    public string OptionB { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option C is required")]
    [StringLength(500)]
    [Display(Name = "Option C")]
    public string OptionC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option D is required")]
    [StringLength(500)]
    [Display(Name = "Option D")]
    public string OptionD { get; set; } = string.Empty;

    [Required(ErrorMessage = "Correct answer is required (A, B, C, or D)")]
    [RegularExpression("^[A-Da-d]$", ErrorMessage = "Correct answer must be A, B, C, or D")]
    [Display(Name = "Correct Answer (A, B, C, or D)")]
    public string CorrectAnswer { get; set; } = "A";
}

public class LearnerItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int EnrolledCoursesCount { get; set; }
    public int CompletedCoursesCount { get; set; }
    public int QuizzesTakenCount { get; set; }
}

public class LearnerDetailViewModel
{
    public ApplicationUser Learner { get; set; } = null!;
    public List<Enrollment> Enrollments { get; set; } = new();
    public List<LessonProgress> LessonProgresses { get; set; } = new();
    public List<QuizResult> QuizResults { get; set; } = new();
}

public class LearnerDeleteViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int EnrolledCoursesCount { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int QuizResultsCount { get; set; }
}

public class EnrollmentDetailViewModel
{
    public Enrollment Enrollment { get; set; } = null!;
    public List<LessonProgress> LessonProgresses { get; set; } = new();
    public QuizResult? QuizResult { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int TotalLessonsCount { get; set; }
}
