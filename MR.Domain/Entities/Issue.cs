namespace MR.Domain.Entities;

[Table(TableNames.Issues, Schema = SchemasNames.MRBasics)]
public class Issue : BaseEntity<Guid>
{
    [ForeignKey(nameof(CreatedBy))]
    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }
    public string Title { get; set; }
    public string Question { get; set; }
    public bool IsVerifyByAdmin { get; set; } = false;
    //bazujac na tym statusie ustawiamy widocznosc
    public IssueVisibility IssueVisibility { get; set; } = IssueVisibility.NotVisible;
    //bazujac na tym statusie ustawiamy widocznosc
    public IssueProcess IssueProcess { get; set; } = IssueProcess.InCreation;
    
    public decimal RatingValue { get; set; } = 0;
    [InverseProperty(nameof(IssueRatingHistory.Issue))]
    public ICollection<IssueRatingHistory> IssueRatingHistories { get; set; }

    [ForeignKey(nameof(InitialPayment))]
    public Guid? InitialPaymentId { get; set; }
    public Payment? InitialPayment { get; set; }

    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }

    //Kwestia moze nalezec do wielu kwartalow, jezeli wlasciel ja np. przedluza
    [InverseProperty(nameof(QuarterIssue.Issue))]
    public ICollection<QuarterIssue> QuarterIssues { get; set; }

    [InverseProperty(nameof(Signature.Issue))]
    public ICollection<Signature> Signatures { get; set; }

    [InverseProperty(nameof(IssueVisibilityHistory.Issue))]
    public ICollection<IssueVisibilityHistory> IssueVisibilityHistories { get; set; }

    [InverseProperty(nameof(IssueProcessingHistory.Issue))]
    public ICollection<IssueProcessingHistory> IssueProcessingHistories { get; set; }

    [InverseProperty(nameof(CloudinaryFileIssue.Issue))]
    public ICollection<CloudinaryFileIssue> CloudinaryFileIssues { get; set; }

}
