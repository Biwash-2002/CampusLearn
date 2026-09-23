using CampusLearn.Models;

namespace CampusLearn.ViewModels;

public class CourseListViewModel
{
    public IEnumerable<Course> Courses { get; set; } = new List<Course>();
    public string? SearchTerm { get; set; }
    public string? SelectedCategory { get; set; }
    public List<string> Categories { get; set; } = new();
    public int TotalCoursesCount { get; set; }
}

public class CourseDetailViewModel
{
    public Course Course { get; set; } = null!;
    public bool IsEnrolled { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int ProgressPercentage { get; set; }
    public int? NextLessonId { get; set; }
    public List<LessonStatusViewModel> Lessons { get; set; } = new();
}

public class LessonStatusViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int LessonOrder { get; set; }
    public bool IsCompleted { get; set; }
}
