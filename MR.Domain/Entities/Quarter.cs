namespace MR.Domain.Entities;

[Table(TableNames.Quarters, Schema = SchemasNames.MRBasics)]
public class Quarter : BaseEntity<Guid>
{
    public int Year { get; set; }
    public int QuarterNumber { get; set; }

    //kwestie rozpatrywane w danym kwartale
    [InverseProperty(nameof(QuarterIssue.Quarter))]
    public ICollection<QuarterIssue> QuarterIssues { get;  set; }

    public int PrimarySignatureCount { get; set; }

    /// <summary>
    /// Zakończenie i ustalenie zwycięzców kwartału obliguje go do definitywnego zamknięcia
    /// </summary>
    public bool QuarterResolved { get; set; }
}
