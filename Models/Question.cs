using System.ComponentModel.DataAnnotations;

namespace CampusLearn.Models;

public class Question
{
    public int QuestionId { get; set; }

    [Required]
    public int QuizId { get; set; }

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

    [Required(ErrorMessage = "Correct answer is required")]
    [StringLength(10)]
    [Display(Name = "Correct Answer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    // Navigation properties
    public virtual Quiz Quiz { get; set; } = null!;
}
