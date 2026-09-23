using System.ComponentModel.DataAnnotations;

namespace CampusLearn.Models;

public class LessonProgress
{
    public int LessonProgressId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int LessonId { get; set; }

    public int? EnrollmentId { get; set; }

    [Display(Name = "Completed")]
    public bool Completed { get; set; } = false;

    [Display(Name = "Completed At")]
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Lesson Lesson { get; set; } = null!;
    public virtual Enrollment? Enrollment { get; set; }
}
