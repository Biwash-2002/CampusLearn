using System.ComponentModel.DataAnnotations;

namespace CampusLearn.Models;

public class Lesson
{
    public int LessonId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Lesson Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [Display(Name = "Lesson Content")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lesson Order is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Lesson order must be a positive number (1 or greater)")]
    [Display(Name = "Lesson Order")]
    public int LessonOrder { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Course Course { get; set; } = null!;
    public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
}
