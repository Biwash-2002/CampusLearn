using System.ComponentModel.DataAnnotations;

namespace CampusLearn.Models;

public class Quiz
{
    public int QuizId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Quiz Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    // Navigation properties
    public virtual Course Course { get; set; } = null!;
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
}
