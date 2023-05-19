using MR.Shared.DTOs.Payment;

namespace MR.Shared.DTOs.Issue;

public class IssueDTO : BaseDTO
{
    //nullable
    public string ApplicationUserEmail { get; set; }

    //not null
    public string Title { get; set; }
    public string Question { get; set; }
    public bool IsVerifyByAdmin { get; set; } = false;
    //bazujac na tym statusie ustawiamy widocznosc
    public IssueStatusEnum IssueStatus { get; set; } = IssueStatusEnum.NotVisible;
    //Rating value na podstawie którego okreslamy miejsce w top10
    public int RatingValue { get; set; } = 0;
    public PaymentDTO? InitialPayment { get; set; }
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
}
