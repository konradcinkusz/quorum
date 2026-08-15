namespace Quorum.Shared.DTOs.Issue;

public class IssueCreateDTO
{
    [Required(ErrorMessage = "Please enter a title.")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 50 characters.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Please enter a Question.")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Question must be between 5 and 50 characters.")]
    public string Question { get; set; }

    [Required(ErrorMessage = "Please choose an icon.")]
    public string? Icon { get; set; }

    [Required(ErrorMessage = "Please background color.")]
    public string? BackgroundColor { get; set; }

    public PaymentDTO? InitialPaymnet { get; set; }
}

