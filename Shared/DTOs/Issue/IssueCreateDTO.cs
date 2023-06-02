namespace MR.Shared.DTOs.Issue;

public class IssueCreateDTO : BaseDTO
{
    public Guid? IssueId { get; set; }
    public string Title { get; set; }
    public string Question { get; set; }
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public PaymentDTO? InitialPaymnet { get; set; }
}

