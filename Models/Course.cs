using System.ComponentModel.DataAnnotations;

namespace CampusLearn.Models;

public class Course
{
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Course Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [Display(Name = "Course Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Duration is required")]
    [StringLength(50, ErrorMessage = "Duration cannot exceed 50 characters")]
    [Display(Name = "Duration")]
    public string Duration { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
    [RegularExpression(@"^(https?:\/\/|\/|~\/)[^\s]+$", ErrorMessage = "Please enter a valid URL (starting with http://, https://, or /)")]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated At")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
