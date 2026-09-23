using CampusLearn.Models;

namespace CampusLearn.ViewModels;

public class LearnerDashboardViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public int TotalEnrolledCourses { get; set; }
    public int CompletedCoursesCount { get; set; }
    public int InProgressCoursesCount { get; set; }
    public int TotalQuizzesPassed { get; set; }
    public int AverageQuizScore { get; set; }
    public int OverallProgressPercentage { get; set; }
    public List<EnrolledCourseProgressViewModel> EnrolledCourses { get; set; } = new();
    public List<RecentQuizResultViewModel> RecentQuizResults { get; set; } = new();
    public List<Course> RecommendedCourses { get; set; } = new();
}

public class EnrolledCourseProgressViewModel
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseDescription { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? Duration { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public int ProgressPercentage { get; set; }
    public string Status { get; set; } = "Active";
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int? NextLessonId { get; set; }
    public string? NextLessonTitle { get; set; }
    public QuizResult? LatestQuizResult { get; set; }
    public int? AvailableQuizId { get; set; }
    public string? AvailableQuizTitle { get; set; }
}

public class RecentQuizResultViewModel
{
    public int QuizResultId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string QuizTitle { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public int Percentage => TotalQuestions > 0 ? (int)Math.Round((double)Score / TotalQuestions * 100) : 0;
    public bool IsPassed => Percentage >= 60;
    public DateTime AttemptDate { get; set; }
}

public class LearnerProgressViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public int OverallProgressPercentage { get; set; }
    public int TotalLessonsCompleted { get; set; }
    public int TotalQuizzesTaken { get; set; }
    public double OverallAverageScore { get; set; }
    public List<CourseProgressDetailViewModel> CourseDetails { get; set; } = new();
}

public class CourseProgressDetailViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; } = "Active";
    public int ProgressPercentage { get; set; }
    public List<LessonProgressItemViewModel> Lessons { get; set; } = new();
}

public class LessonProgressItemViewModel
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int LessonOrder { get; set; }
    public bool Completed { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class LessonViewViewModel
{
    public Lesson Lesson { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public bool Completed { get; set; }
    public int ProgressPercentage { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int TotalLessonsCount { get; set; }
    public Lesson? PreviousLesson { get; set; }
    public Lesson? NextLesson { get; set; }
    public List<LessonNavViewModel> AllCourseLessons { get; set; } = new();
    public List<Quiz> CourseQuizzes { get; set; } = new();
}

public class LessonNavViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int LessonOrder { get; set; }
    public bool Completed { get; set; }
    public bool IsCurrent { get; set; }
}

public class QuizTakingViewModel
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public string? QuizDescription { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public List<QuizQuestionAnswerViewModel> Questions { get; set; } = new();
}

public class QuizQuestionAnswerViewModel
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string? SelectedOption { get; set; }
}

public class QuizResultDetailViewModel
{
    public QuizResult Result { get; set; } = null!;
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public string? QuizDescription { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public List<QuizQuestionFeedbackViewModel> QuestionFeedbacks { get; set; } = new();
    public int Percentage => Result.TotalQuestions > 0 ? (int)Math.Round((double)Result.Score / Result.TotalQuestions * 100) : 0;
    public bool IsPassed => Percentage >= 60;
}

public class QuizQuestionFeedbackViewModel
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? UserAnswer { get; set; }
    public bool IsCorrect => !string.IsNullOrEmpty(UserAnswer) && string.Equals(UserAnswer.Trim(), CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
}
