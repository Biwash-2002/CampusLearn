using System.ComponentModel.DataAnnotations;

namespace CampusLearn.Models;

public class Enrollment
{
    public int EnrollmentId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int CourseId { get; set; }

    [Display(Name = "Enrollment Date")]
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    [Range(0, 100)]
    [Display(Name = "Progress Percentage")]
    public int ProgressPercentage { get; set; } = 0;

    [Required]
    [StringLength(50)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Active";

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Course Course { get; set; } = null!;
    public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
}
