using System.ComponentModel.DataAnnotations;

namespace CampusLearn.Models;

public class QuizResult
{
    public int QuizResultId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int QuizId { get; set; }

    [Required]
    [Display(Name = "Score")]
    public int Score { get; set; }

    [Required]
    [Display(Name = "Total Questions")]
    public int TotalQuestions { get; set; }

    [Display(Name = "Attempt Date")]
    public DateTime AttemptDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Quiz Quiz { get; set; } = null!;
}
