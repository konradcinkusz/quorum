namespace Quorum.Domain.Entities;

[Table(TableNames.Issues, Schema = SchemasNames.MRBasics)]
public class Issue : BaseEntity<Guid>
{
    /// <summary>
    /// The subject id of the user who filed the issue, as issued by the identity service.
    /// A plain string key on purpose: identity lives in <c>authservice</c> (ADR 0001) and
    /// P3 forbids a navigation into another service's database.
    /// </summary>
    public string? CreatedById { get; set; }

    /// <summary>
    /// The creator's email address as it stood when the issue was filed, captured from the
    /// authenticated caller's <c>email</c> claim.
    /// <para>
    /// Denormalised deliberately. Identity lives in <c>authservice</c>
    /// (<see href="https://github.com/konradcinkusz/quorum/blob/master/docs/architecture/0001-identity-via-authservice.md">ADR 0001</see>)
    /// so there is no user row here to join to — P3 forbids reaching into
    /// another service's database, and IDENTITY-AND-ACCOUNTS §1 is explicit that a service
    /// holding a token does not call back to ask about the user.
    /// </para>
    /// <para>
    /// For a petition this is the more correct model regardless of where identity lives: a
    /// signature sheet should record who filed the initiative <i>at the time of filing</i>.
    /// A later email change does not, and should not, rewrite documents people have already
    /// signed. The accepted cost is that this value can go stale against the identity
    /// service; that staleness is the point.
    /// </para>
    /// </summary>
    public string? CreatedByEmail { get; set; }
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
