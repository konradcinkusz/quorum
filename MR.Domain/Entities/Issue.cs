namespace MR.Domain.Entities;

[Table(TableNames.Issues, Schema = SchemasNames.MRBasics)]
public class Issue : BaseEntity<Guid>
{
    //nullable
    [ForeignKey(nameof(Issue.CreatedBy))]
    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }

    //not null
    public string Title { get; set; }
    public string Question { get; set; }
    public bool IsVerifyByAdmin { get; set; } = false;
    //bazujac na tym statusie ustawiamy widocznosc
    public IssueStatus IssueStatus { get; set; } = IssueStatus.NotVisible;
    //Rating value na podstawie którego okreslamy miejsce w top10
    public int RatingValue { get; set; } = 0;
    public Payment? InitialPayment { get; set; }

    //Kwestia moze nalezec do wielu kwartalow, jezeli wlasciel ja np. przedluza
    [InverseProperty(nameof(QuarterIssue.Issue))]
    public ICollection<QuarterIssue> QuarterIssues { get; set; }

    [InverseProperty(nameof(Signature.Issue))]
    public ICollection<Signature> Signatures { get; set; }
}
